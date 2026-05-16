using UnityEngine;

public abstract class BaseState 
{
    public abstract void Enter();
    public abstract void Update();
    public abstract void FixedUpdate();
    public abstract void FixedUpdateNetwork();
    public abstract void Exit();
    public abstract void OnDrawGizmos();
    public abstract bool TryChangeState();
}
