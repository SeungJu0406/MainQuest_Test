using NSJ_Player;
using UnityEngine;

public class ThrowState : PlayerState
{
    private float _throwDuration = 0.438f; //하드코딩, 애니메이션 길이와 강제로 맞춤

    private float _throwTimer = 0f;
    public ThrowState(PlayerController player) : base(player)
    {
    }

    public override void Enter()
    {
       View.RPC_PlayAnimation("Throw");

        _throwTimer = 0;
    }

    public override void Exit()
    {
        _throwTimer = 0;
    }
    public override void Update()
    {
        _throwTimer += Time.deltaTime;
        if (_throwTimer > _throwDuration)
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
       return false;
    }


}
