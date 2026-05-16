using UnityEngine;

namespace NSJ_Player
{
    public abstract class PlayerState : BaseState
    {
        public enum State
        {
            Idle,
            Move,
            Attack,
            Throw,
            Charging,
            Hit,
            SIZE
        }

        protected PlayerController Player;
        protected PlayerView View => Player.View;

        public PlayerState(PlayerController player)
        {
            Player = player;
        }

        protected GameObject gameObject => Player.gameObject;
        protected Transform transform => Player.transform;
        
        protected Rigidbody2D Rb => Player.Rb;
        protected Vector2 MoveDir => Player.MoveDir;
        protected State CurState => Player.CurState;


        public abstract void OnTrigger();
        protected void ChangeState(State state) => Player.ChangeState(state);

        protected void ReadInput()
        {
            Player.MoveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
            if (Player.MoveDir.x != 0)
            {
                int newFacing = Player.MoveDir.x > 0 ? 1 : -1;
                if (newFacing != Player.FacingDirX)
                {
                    Player.FacingDirX = newFacing;
                    Player.RPC_BroadcastSpriteFlipX(newFacing < 0);
                }
            }
        }

        protected void ApplyMovement() => Rb.linearVelocity = Player.MoveDir * Player.MoveSpeed;
        protected void StopMovement()  => Rb.linearVelocity = Vector2.zero;

        protected void CheckCombat()
        {
            var nearby = Player.Battle.Nearby;
            if (nearby == null) return;

            if (!nearby.IsStunned)
            {
                if (Input.GetKeyDown(KeyCode.Space))
                {
                    nearby.RPC_GetHit();
                    ChangeState(State.Attack);
                }
            }
            else
            {
                if (Input.GetKey(KeyCode.Space))
                    ChangeState(State.Charging);
            }
        }

    }
}