using UnityEngine;

namespace NSJ_Player
{
    public class MoveState : PlayerState
    {
        float _timer = 0f;
        public MoveState(PlayerController player) : base(player)
        {
        }

        public override void Enter()
        {
            View.RPC_PlayAnimation("Walk");
        }

        public override void Exit()
        {

        }

        public override void FixedUpdate()
        {

        }

        public override void FixedUpdateNetwork()
        {
           
        }

        public override void OnDrawGizmos()
        {

        }

        public override void OnTrigger()
        {

        }

        public override bool TryChangeState()
        {
            return true;
        }

        public override void Update()
        {
            //CheckInput();
            //Move();
        }


        private void CheckInput()
        {
            //if (MoveDir.x == 0)
            //{
            //    ChangeState(State.Idle);
            //}
            //if (Player.IsGrounded == false)
            //{
            //    ChangeState(State.Fall);
            //}
        }

        private void Move()
        {

        }
    }
}