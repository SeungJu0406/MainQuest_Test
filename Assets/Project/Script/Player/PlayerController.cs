using Fusion;
using NSJ_Player;
using System;
using System.Collections;
using UnityEngine;
using Utility;

public class PlayerController : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private GameObject _ownerIndicator;
    [Serializable]
    public struct OwnerIndicatorMoveValue
    {
        public float MaxY;
        public float MinY;
        public float Speed;
        public bool IsUp;
    }
    [SerializeField] private OwnerIndicatorMoveValue _ownerIndicatorMoveValue;

    [SerializeField] private ColorPalette _colorPalette;
    [SerializeField] private PlayerBattleModule _battle;
    [SerializeField] private PlayerStateMachine _stateMachine;
    [SerializeField] private float _moveSpeed = 5f;

    [HideInInspector] public Rigidbody2D Rb;
    [HideInInspector] public Vector2 MoveDir;
    [HideInInspector] public int FacingDirX = 1;
    [Networked] public PlayerState.State CurState { get; set; }
    [HideInInspector] public PlayerView View;

    public float MoveSpeed => _moveSpeed;
    public PlayerBattleModule Battle => _battle;
    public bool IsStunned => _battle.IsStunned;

    private ChangeDetector _changes;
    private bool _colorRequested;
    private SpriteRenderer _renderer;

    private void Awake()
    {
        Rb = GetComponent<Rigidbody2D>();
        _renderer = GetComponent<SpriteRenderer>();
        View = GetComponent<PlayerView>();
    }

    private void Start()
    {
        StartCoroutine(MilliSecondUpdate());
    }

    public override void Spawned()
    {
        Rb = GetComponent<Rigidbody2D>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        ApplyColor(ColorIndex);
        _battle.Init(this, Rb, Runner);
        _stateMachine.Initialize(this);

        if (HasStateAuthority)
            Owner = Runner.LocalPlayer;
        _ownerIndicator.SetActive(Owner == Runner.LocalPlayer);

        if (!HasStateAuthority) return;

        ChangeState(PlayerState.State.Idle);
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!hasState) return;
        var spawner = Manager.PlayerSpawner;
        if (spawner == null) return;
        spawner.RPC_EnqueueColor(ColorIndex);
    }

    private IEnumerator MilliSecondUpdate()
    {
        while (true)
        {
            if (HasStateAuthority)
                _battle.NearbyDetectionUpdate();
            yield return 0.05f.Second();
        }
    }

    private void Update()
    {
        if (!HasStateAuthority) return;

        MoveOwnIndicator();
        _stateMachine.Update();
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

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

        _stateMachine.FixedUpdateNetwork();
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out _, out _))
        {
            if (change == nameof(ColorIndex))
                ApplyColor(ColorIndex);
        }
    }

    private void OnCollisionEnter2D(Collision2D col)
    {
        if (!HasStateAuthority) return;
        if (col.gameObject.layer == LayerMask.NameToLayer("Wall"))
            _battle.OnWallHit();
    }

    public void ChangeState(PlayerState.State state, Action callback = null) => _stateMachine.ChangeState(state, callback);
    public PlayerState GetCurState() => _stateMachine.GetCurState();
    public PlayerState GetPrevState() => _stateMachine.GetPrevState();

    private void MoveOwnIndicator()
    {
        if (!_ownerIndicator.activeSelf) return;

        _ownerIndicator.transform.localPosition = new Vector3(
            _ownerIndicator.transform.localPosition.x,
            _ownerIndicator.transform.localPosition.y + _ownerIndicatorMoveValue.Speed * (_ownerIndicatorMoveValue.IsUp ? 1 : -1) * Time.deltaTime,
            _ownerIndicator.transform.localPosition.z);

        if (_ownerIndicator.transform.localPosition.y >= _ownerIndicatorMoveValue.MaxY)
            _ownerIndicatorMoveValue.IsUp = false;
        else if (_ownerIndicator.transform.localPosition.y <= _ownerIndicatorMoveValue.MinY)
            _ownerIndicatorMoveValue.IsUp = true;
    }

    public void SetNearbyIndicator(bool show, bool showCharge) => _battle.SetNearbyIndicator(show, showCharge);
    public void UpdateChargeSliderValue(float value) => _battle.UpdateChargeSliderValue(value);

    // ──────────────────────────────────────────
    // RPC
    // ──────────────────────────────────────────

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetHit() => _battle.OnHit(_battle.HitStunDuration);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BroadcastStunned(NetworkBool stunned)
    {
        _battle.OnStunned(stunned);

        if (HasStateAuthority == false) return;
        _stateMachine.ChangeState(stunned ? PlayerState.State.Hit : PlayerState.State.Idle);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetThrown(float directionX, float force) => _battle.OnThrown(directionX, force);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BroadcastSpriteFlipX(bool isLeft)
    {
        if (_renderer == null) return;
        _renderer.flipX = isLeft;
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
