using Fusion;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerBattleModule
{
    [Header("전투")]
    [SerializeField] private float _attackRadius = 1.5f;
    [SerializeField] private float _hitStunDuration = 2f;
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

    // PlayerController.Update()에서 호출 — HasStateAuthority, !IsStunned 보장
    public void UpdateBattle(int facingDirX)
    {
        if (_nearby == null) return;

        if (!_nearby.IsStunned)
        {
            if (_isCharging) ResetCharge();

            if (Input.GetKeyDown(KeyCode.Space))
                _nearby.RPC_GetHit();
        }
        else
        {
            if (Input.GetKey(KeyCode.Space))
            {
                _isCharging = true;
                _chargeValue = Mathf.Min(_chargeValue + Time.deltaTime / _maxChargeTime, 1f);
                _nearby.UpdateChargeSliderValue(_chargeValue);
            }

            if (Input.GetKeyUp(KeyCode.Space) && _isCharging)
                TryThrow(_nearby, facingDirX);
        }
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

    private void TryThrow(PlayerController target, int facingDirX)
    {
        float force = _chargeValue * _maxThrowForce;
        ResetCharge();
        target.RPC_GetThrown(facingDirX, force);
    }
}
