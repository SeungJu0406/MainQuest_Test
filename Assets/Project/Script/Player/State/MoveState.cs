using UnityEngine;

namespace NSJ_Player
{
    public class MoveState : PlayerState
    {
        public MoveState(PlayerController player) : base(player) { }

        public override void Enter()
        {
            View.RPC_PlayAnimation("Walk");
        }

        public override void Exit() { }

        public override void Update()
        {
            ReadInput();
            CheckCombat();
            if (MoveDir.x == 0 && MoveDir.y == 0)
                ChangeState(State.Idle);
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
