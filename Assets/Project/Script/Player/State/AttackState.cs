using NSJ_Player;
using UnityEngine;

public class AttackState : PlayerState
{
    private float _attackDuration = 0.375f; // 하드 코딩, 영상 애니메이션에 맞춤

    private float _attackTimer = 0f;

    public AttackState(PlayerController player) : base(player)
    {

    }

    public override void Enter()
    {
       View.RPC_PlayAnimation("Attack");

        _attackTimer = 0;
    }

    public override void Exit()
    {
        _attackTimer = 0;
    }
    public override void Update()
    {
        _attackTimer += Time.deltaTime;
        if(_attackTimer > _attackDuration)
        {
            ChangeState(State.Idle);
        }
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


}
