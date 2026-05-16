using System;
using UnityEngine.Events;

namespace NSJ_Player
{
    [System.Serializable]
    public class PlayerStateMachine
    {
        public PlayerState.State PrevState;
        public PlayerState.State CurState { get { return _controller.CurState; } set { _controller.CurState = value; } }

        private PlayerController _controller;

        private PlayerState[] _states;

        public event UnityAction OnStateChanged;

        public PlayerStateMachine(PlayerController controller)
        {
            Initialize(controller);
        }

        public void Initialize(PlayerController controller)
        {
            _controller = controller;
            BindState();
            PrevState = PlayerState.State.Idle;
            CurState = PlayerState.State.Idle;
        }

        public PlayerState GetCurState()
        {
            return _states[(int)CurState];
        }

        public PlayerState GetPrevState()
        {
            return _states[(int)PrevState];
        }
        /// <summary>
        /// 상태를 교체합니다.
        /// </summary>
        /// <param name="state"></param>
        public void ChangeState(PlayerState.State state, Action callback = null)
        {
            if (_states[(int)CurState].TryChangeState() == false)
                return;

            _states[(int)CurState].Exit();
            PrevState = CurState;
            CurState = state;
            callback?.Invoke();
            OnStateChanged?.Invoke();

            _states[(int)CurState].Enter();

        }

        public void Update()
        {
            _states[(int)CurState].Update();
        }
        public void FixedUpdate()
        {
            _states[(int)CurState].FixedUpdate();
        }
        public void FixedUpdateNetwork()
        {
            _states[(int)CurState].FixedUpdateNetwork();
        }   
        public void OnDrawGizmos()
        {
            if (_states == null)
                return;
            if (_states[(int)CurState] == null)
                return;
            _states[(int)CurState].OnDrawGizmos();
        }
        public void OnTrigger()
        {
            _states[(int)CurState].OnTrigger();
        }

        /// <summary>
        /// 상태 객체를 만듭니다
        /// </summary>
        private void BindState()
        {
            _states = new PlayerState[(int)PlayerState.State.SIZE];

            _states[(int)PlayerState.State.Idle] = new IdleState(_controller);
            _states[(int)PlayerState.State.Move] = new MoveState(_controller);
            _states[(int)PlayerState.State.Attack] = new AttackState(_controller);
            //[(int)PlayerState.State.Throw] = new ThrowState(_controller);
        }
    }
}