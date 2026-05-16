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


        public override bool TryChangeState()
        {
            return true;
        }

        public override void OnTrigger()
        {

        }


    }
}