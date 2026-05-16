using Fusion;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Utility;

public class PlayerController : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private ColorPalette _colorPalette;

    [SerializeField] private PlayerBattleModule _battleModule;
    [SerializeField] private PlayerMoveModule _moveModule;

    [SerializeField] private float _moveSpeed = 5f;

    [Header("전투")]
    [SerializeField] private float _attackRadius = 1.5f;
    [SerializeField] private float _hitStunDuration = 2f;    // 맞았을 때 기절 시간
    [SerializeField] private float _wallStunDuration = 1.5f; // 벽 충돌 후 기절 시간
    [SerializeField] private float _maxThrowForce = 15f;
    [SerializeField] private float _maxChargeTime = 2f;
    [SerializeField] private GameObject _nearbyIndicator;    // 상대 플레이어 프리팹의 자식 UI
    [SerializeField] private GameObject _stunText;          // 기절 상태 표시 UI
    [SerializeField] private Slider _chargeSlider;        // 차징 상태 표시 UI

    private Vector2 _moveDir;
    private Rigidbody2D _rb;
    private ChangeDetector _changes;
    private bool _colorRequested;

    private PlayerController _nearby;   // 근처 플레이어 캐싱 (로컬 전용)

    // 기절 상태 (RPC_BroadcastStunned로 전체 동기화)
    public bool IsStunned { get; private set; }
    private bool _isThrown;       // 날아가는 중 — 벽 감지용
    private float _stunTimer;     // 피격자 로컬 타이머

    // 공격자 로컬 상태
    private float _chargeValue;
    private bool _isCharging;
    private int _facingDirX = 1;  // 마지막 이동 방향 (-1: 왼쪽, 1: 오른쪽)

    private void Awake()
    {
        _battleModule = new PlayerBattleModule(this);
        _moveModule = new PlayerMoveModule(this);

    }

    private void Start()
    {
        StartCoroutine(MilliSecondUpdate());
    }

    public override void Spawned()
    {
        ApplyColor(ColorIndex);

        _rb = GetComponent<Rigidbody2D>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
            Owner = Runner.LocalPlayer;
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!hasState) return;
        var spawner = Manager.PlayerSpawner;
        if (spawner == null) return;
        spawner.RPC_EnqueueColor(ColorIndex);
    }

    private void Update()
    {
        if (!HasStateAuthority) return;

        // 이동 입력 + facing 방향 추적
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (_moveDir.x != 0)
            _facingDirX = _moveDir.x > 0 ? 1 : -1;

        if (IsStunned)
        {
            ResetChargeValue();
            return;
        }


        if (_nearby == null) return;

        if (!_nearby.IsStunned)
        {
            // 차지상태라면 밸류 초기화
            ResetChargeValue();

            // 기절 공격
            if (Input.GetKeyDown(KeyCode.Space))
                _nearby.RPC_GetHit();
        }
        else
        {
            // 차징
            if (Input.GetKey(KeyCode.Space))
            {
                _isCharging = true;
                _chargeValue = Mathf.Min(_chargeValue + Time.deltaTime / _maxChargeTime, 1f);
                UpdateChargeSlider(_nearby, _chargeValue);
            }

            // 던지기
            if (Input.GetKeyUp(KeyCode.Space) && _isCharging)
                TryThrow(_nearby);
        }
    }

    private IEnumerator MilliSecondUpdate()
    {
        while (true)
        {
            if (!HasStateAuthority) { yield return null; continue; }

            // 근접 플레이어 감지
            PlayerController curNearby = FindNearbyPlayer();
            // 인디케이터: 내 화면에서 근처 플레이어 표시
            if(curNearby != _nearby)
            {
                // 차지상태 리셋
                _isCharging = false;
                _chargeValue = 0;
            }

            _nearby = curNearby;
            UpdateNearbyIndicator(_nearby);

            yield return 0.05f.Second();
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        // 기절 중: 타이머 감산, 이동 차단
        if (IsStunned)
        {
            _stunTimer -= Runner.DeltaTime;
            if (_stunTimer <= 0f)
                RPC_BroadcastStunned(false);
            return;
        }

        if (!_colorRequested)
        {
            var spawner = Manager.PlayerSpawner;
            if (spawner == null || !spawner.IsSpawnedReady || spawner.AvailableColorIndex.Count == 0)
                return;

            _colorRequested = true;

            int colorIndex = spawner.AvailableColorIndex[spawner.AvailableColorIndex.Count - 1];
            ColorIndex = colorIndex;
            spawner.RPC_DequeueColor(colorIndex);
        }

        _rb.linearVelocity = _moveDir * _moveSpeed;
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out _, out _))
        {
            if (change == nameof(ColorIndex))
                ApplyColor(ColorIndex);
        }
    }

    // ──────────────────────────────────────────
    // 전투 로직
    // ──────────────────────────────────────────

    private PlayerController FindNearbyPlayer()
    {
        var hits = Physics2D.OverlapCircleAll(transform.position, _attackRadius);
        foreach (var hit in hits)
        {
            var pc = hit.GetComponent<PlayerController>();
            if (pc == null || pc == this) continue;
            return pc;
        }
        return null;
    }

    // 상대 플레이어의 인디케이터를 내 화면에서만 제어 (로컬 전용)
    private void UpdateNearbyIndicator(PlayerController nearby)
    {
        foreach (var netObj in Runner.GetAllNetworkObjects())
        {
            var pc = netObj.GetComponent<PlayerController>();
            if (pc == null || pc == this) continue;
            if (pc._nearbyIndicator != null)
            {
                pc._nearbyIndicator.SetActive(pc == nearby);
                pc._chargeSlider.gameObject.SetActive(pc == nearby && _isCharging && nearby.IsStunned == true);
            }

        }
    }

    // 차징 중 상대플레이어 위 슬라이더 표시(로컬 전용)
    private void UpdateChargeSlider(PlayerController nearby, float chargeValue)
    {
        if (_chargeSlider == null) return;
        if (nearby == null) return;

        nearby.UpdateChargeSliderValue(chargeValue);
    }
    public void UpdateChargeSliderValue(float value)
    {
        if (_chargeSlider == null) return;
        

        _chargeSlider.value = value;
    }


    private void TryThrow(PlayerController target)
    {
        float force = _chargeValue * _maxThrowForce;
        _chargeValue = 0f;
        _isCharging = false;
        target.RPC_GetThrown(_facingDirX, force);
    }

    private void ResetChargeValue()
    {
        // 차지상태라면 밸류 초기화
        if (_isCharging == true)
        {
            _isCharging = false;
            _chargeValue = 0f;
        }
    }

    // 날아가다 벽에 충돌 — 피격자 클라이언트에서만 처리
    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!HasStateAuthority || !_isThrown) return;
        if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            _isThrown = false;
            _rb.linearVelocity = Vector2.zero;
            _stunTimer = _wallStunDuration;  // 벽 충돌 후 기절 시간으로 리셋
        }
    }

    // ──────────────────────────────────────────
    // RPC
    // ──────────────────────────────────────────

    // 공격자 → 피격자: 기절 요청
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetHit()
    {
        _stunTimer = _hitStunDuration;
        RPC_BroadcastStunned(true);
    }

    // 피격자 → 전체: 기절 상태 동기화
    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BroadcastStunned(NetworkBool stunned)
    {
        IsStunned = stunned;
        if (stunned == false)
        {
            _isThrown = false;
            _rb.linearVelocity = Vector2.zero;
            _stunText.SetActive(false);
        }
        else
        {
            _stunText.SetActive(true);
        }
    }

    // 공격자 → 피격자: 던지기 요청
    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetThrown(float directionX, float force)
    {
        _isThrown = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(new Vector2(directionX, 0f) * force, ForceMode2D.Impulse);
    }

    // ──────────────────────────────────────────
    // 유틸
    // ──────────────────────────────────────────

    private void ApplyColor(int index)
    {
        if (_colorPalette == null || index < 0 || index >= _colorPalette.Colors.Length) return;
        var rend = GetComponent<Renderer>();
        if (rend) rend.material.color = _colorPalette.Colors[index];
    }
}
