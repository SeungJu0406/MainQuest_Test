using NSJ_Player;
using UnityEngine;

public class HitState : PlayerState
{
    public HitState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Battle.ResetCharge();
        View.RPC_PlayAnimation("Hit");
    }

    public override void Exit() { }

    public override void Update() { }

    public override void FixedUpdate() { }

    public override void FixedUpdateNetwork()
    {
        StopMovement();
        if (Player.Battle.TickStunTimer(Runner.DeltaTime))
            Player.RPC_BroadcastStunned(false);
    }

    public override void OnDrawGizmos() { }
    public override void OnTrigger() { }

    // true여야 스턴 회복 시 Idle로 빠져나올 수 있음
    public override bool TryChangeState() => true;
}
