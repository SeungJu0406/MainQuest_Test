using NSJ_Player;
using UnityEngine;

public class ChargingState : PlayerState
{
    public ChargingState(PlayerController player) : base(player) { }

    public override void Enter()
    {
        Player.Battle.ResetCharge();
    }

    public override void Exit()
    {
        Player.Battle.ResetCharge();
    }

    public override void Update()
    {
        var nearby = Player.Battle.Nearby;

        // 근처 기절 상태 해제되면 차징 취소
        if (nearby == null || !nearby.IsStunned)
        {
            ChangeState(State.Idle);
            return;
        }

        if (Input.GetKey(KeyCode.Space))
        {
            Player.Battle.AccumulateCharge(Time.deltaTime);
            nearby.UpdateChargeSliderValue(Player.Battle.ChargeValue);
        }

        if (Input.GetKeyUp(KeyCode.Space))
        {
            Player.Battle.ExecuteThrow(Player.FacingDirX);
            ChangeState(State.Throw);
        }
    }

    public override void FixedUpdateNetwork()
    {
        StopMovement();
    }

    public override void FixedUpdate() { }
    public override void OnDrawGizmos() { }
    public override void OnTrigger() { }
    public override bool TryChangeState() => true;
}
