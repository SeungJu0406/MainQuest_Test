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
        protected void ChangeState(State state)
        {
            Player.ChangeState(state);
        }

    }
}