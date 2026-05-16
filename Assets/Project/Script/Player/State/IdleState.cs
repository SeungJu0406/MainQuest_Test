using UnityEngine;

namespace NSJ_Player
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            View.RPC_PlayAnimation("Idle");
        }

        public override void Exit() { }

        public override void Update()
        {
            ReadInput();
            CheckCombat();
            if (MoveDir.x != 0 || MoveDir.y != 0)
                ChangeState(State.Move);
        }

        public override void FixedUpdateNetwork()
        {
            ApplyMovement();
        }

        public override void FixedUpdate() { }
        public override void OnDrawGizmos() { }
        public override void OnTrigger() { }
        public override bool TryChangeState() => true;
    }
}
