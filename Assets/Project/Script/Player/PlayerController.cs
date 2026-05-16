using Fusion;
using System.Collections;
using UnityEngine;
using Utility;

public class PlayerController : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private ColorPalette _colorPalette;
    [SerializeField] private PlayerMoveModule _move;
    [SerializeField] private PlayerBattleModule _battle;

    private Rigidbody2D _rb;
    private ChangeDetector _changes;
    private bool _colorRequested;

    public bool IsStunned => _battle.IsStunned;

    private void Start()
    {
        StartCoroutine(MilliSecondUpdate());
    }

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        ApplyColor(ColorIndex);
        _move.Init(_rb);
        _battle.Init(this, _rb, Runner);

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

        _move.ReadInput();

        if (_battle.IsStunned)
        {
            _battle.ResetCharge();
            return;
        }

        _battle.UpdateBattle(_move.FacingDirX);
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (_battle.IsStunned)
        {
            if (_battle.TickStunTimer(Runner.DeltaTime))
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

        _move.ApplyMovement();
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

    // 다른 플레이어의 BattleModule이 이 플레이어의 인디케이터/슬라이더를 제어할 때 사용
    public void SetNearbyIndicator(bool show, bool showCharge) => _battle.SetNearbyIndicator(show, showCharge);
    public void UpdateChargeSliderValue(float value) => _battle.UpdateChargeSliderValue(value);

    // ──────────────────────────────────────────
    // RPC
    // ──────────────────────────────────────────

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetHit() => _battle.OnHit(_battle.HitStunDuration);

    [Rpc(RpcSources.StateAuthority, RpcTargets.All)]
    public void RPC_BroadcastStunned(NetworkBool stunned) => _battle.OnStunned(stunned);

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_GetThrown(float directionX, float force) => _battle.OnThrown(directionX, force);

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
