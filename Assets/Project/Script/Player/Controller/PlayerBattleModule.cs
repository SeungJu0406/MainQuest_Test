using Fusion;
using NSJ_Player;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerBattleModule
{
    [Header("전투")]
    [SerializeField] private float _attackRadius = 1.5f;
    [SerializeField] private float _hitStunDuration = 2f;
    [SerializeField] private float _hitStopDuration = 0.2f;
    [SerializeField] private float _wallStunDuration = 1.5f;
    [SerializeField] private float _maxThrowForce = 15f;
    [SerializeField] private float _maxChargeTime = 2f;
    [SerializeField] private GameObject _nearbyIndicator;
    [SerializeField] private GameObject _stunText;
    [SerializeField] private Slider _chargeSlider;

    private PlayerController _controller;
    private Rigidbody2D _rb;
    private NetworkRunner _runner;

    public bool IsStunned { get; private set; }
    public float HitStunDuration => _hitStunDuration;
    public float ChargeValue => _chargeValue;
    public PlayerController Nearby => _nearby;

    private bool _isThrown;
    private float _stunTimer;
    private float _chargeValue;
    private bool _isCharging;
    private PlayerController _nearby;

    public void Init(PlayerController controller, Rigidbody2D rb, NetworkRunner runner)
    {
        _controller = controller;
        _rb = rb;
        _runner = runner;

        _nearbyIndicator.SetActive(false);
        _stunText.SetActive(false);
        _chargeSlider.gameObject.SetActive(false);
    }

    // PlayerController의 MilliSecondUpdate 코루틴에서 50ms마다 호출
    public void NearbyDetectionUpdate()
    {
        if (_controller == null) return;

        PlayerController curNearby = FindNearbyPlayer();
        if (curNearby != _nearby)
            ResetCharge();

        _nearby = curNearby;
        UpdateNearbyIndicator(_nearby);
    }

    // ChargingState에서 호출 — 차징 값 누적
    public void AccumulateCharge(float dt)
    {
        _isCharging = true;
        _chargeValue = Mathf.Min(_chargeValue + dt / _maxChargeTime, 1f);
    }

    // ChargingState에서 호출 — 던지기 실행
    public void ExecuteThrow(int facingDirX)
    {
        if (_nearby == null) return;
        float force = _chargeValue * _maxThrowForce;
        ResetCharge();
        _nearby.RPC_GetThrown(facingDirX, force);
    }

    // 기절 타이머 감산. true 반환 시 → PlayerController가 RPC_BroadcastStunned(false) 호출
    public bool TickStunTimer(float deltaTime)
    {
        _stunTimer -= deltaTime;
        return _stunTimer <= 0f;
    }

    public void ResetCharge()
    {
        _isCharging = false;
        _chargeValue = 0f;
    }

    // ── RPC 핸들러 ──

    public void OnHit(float stunDuration)
    {
        _stunTimer = stunDuration;
        _controller.RPC_BroadcastStunned(true);

        // 화면 경직
        HitStop.Instance.Do(_hitStopDuration);
    }

    public void OnStunned(bool stunned)
    {
        IsStunned = stunned;
        if (_stunText != null) _stunText.SetActive(stunned);
        if (!stunned)
        {
            _isThrown = false;
            _rb.linearVelocity = Vector2.zero;
        }
    }

    public void OnThrown(float directionX, float force)
    {
        _isThrown = true;
        _rb.linearVelocity = Vector2.zero;
        _rb.AddForce(new Vector2(directionX, 0f) * force, ForceMode2D.Impulse);
    }

    public void OnWallHit()
    {
        _isThrown = false;
        _rb.linearVelocity = Vector2.zero;
        _stunTimer = _wallStunDuration;
        _controller.RPC_BroadcastStunned(true);
    }

    // ── 인디케이터 / 슬라이더 (PlayerController에서 위임받아 호출) ──

    public void SetNearbyIndicator(bool show, bool showCharge)
    {
        if (_nearbyIndicator != null) _nearbyIndicator.SetActive(show);
        if (_chargeSlider != null) _chargeSlider.gameObject.SetActive(showCharge);
    }

    public void UpdateChargeSliderValue(float value)
    {
        if (_chargeSlider != null) _chargeSlider.value = value;
    }

    // ── 내부 로직 ──

    private PlayerController FindNearbyPlayer()
    {
        var hits = Physics2D.OverlapCircleAll(_controller.transform.position, _attackRadius);
        foreach (var hit in hits)
        {
            var pc = hit.GetComponent<PlayerController>();
            if (pc == null || pc == _controller) continue;
            return pc;
        }
        return null;
    }

    private void UpdateNearbyIndicator(PlayerController nearby)
    {
        foreach (var netObj in _runner.GetAllNetworkObjects())
        {
            var pc = netObj.GetComponent<PlayerController>();
            if (pc == null || pc == _controller) continue;
            bool isNearby = pc == nearby;
            bool showCharge = isNearby && _isCharging && nearby != null && nearby.IsStunned;
            pc.SetNearbyIndicator(isNearby, showCharge);
        }
    }
}
