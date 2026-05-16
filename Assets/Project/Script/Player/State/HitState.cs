using NSJ_Player;
using UnityEngine;

public class HitState : PlayerState
{
    public HitState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
        View.RPC_PlayAnimation("Hit");
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
     
    }
}
