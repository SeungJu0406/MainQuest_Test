using Unity.VisualScripting;
using UnityEngine;

namespace NSJ_Player
{
    public class IdleState : PlayerState
    {
        public IdleState(PlayerController player) : base(player)
        {
        }

        public override void Enter()
        {
           View.RPC_PlayAnimation("Idle");
        }

        public override void Exit()
        {

        }
        public override void Update()
        {
            CheckInput();
        }
        public override void FixedUpdateNetwork()
        {

        }
        public override void FixedUpdate()
        {

        }
        public override void OnDrawGizmos()
        {

        }

        private void CheckInput()
        {
            if (MoveDir.x != 0)
            {
                ChangeState(State.Move);
            }
 
        }


        public override bool TryChangeState()
        {
            return true;
        }

        public override void OnTrigger()
        {

        }


    }
}