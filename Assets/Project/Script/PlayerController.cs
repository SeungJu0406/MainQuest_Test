using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    PlayerPhysicController _physic;


    public override void Spawned()
    {
        if (HasStateAuthority == false) return;

        _physic = GetComponentInChildren<PlayerPhysicController>();

        _physic.IsMyPlayer = true; // 플레이어 설정
        _physic.transform.SetParent(null); //부모 나가기
        transform.position = _physic.transform.position; // 초기 위치를 플레이어 위치로
    }

    public override void FixedUpdateNetwork()
    {
        if(HasStateAuthority == false) return;
        MovePlayer();
    }

    /// <summary>
    /// 플레이어 이동
    /// </summary>
    void MovePlayer()
    {
        if (HasStateAuthority)
        {
            // 내 플레이어면 내 플레이어 모델이 물리처리 오브젝트 위치로
            transform.position = _physic.transform.position;
            transform.rotation = _physic.transform.rotation;
        }
        else
        {
            // 내 플레이어가 아니면 물리처리 오브젝트가 플레이어 모델 위치로
            _physic.transform.position = transform.position;
            _physic.transform.rotation = transform.rotation;
        }
    }

}
