using Fusion;
using System;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f; // 이동 속도

    Vector2 _moveDir;
    Rigidbody2D _rb;

    //GameObject _moveSuppoter;
    public override void Spawned()
    {
        if (HasStateAuthority == false) return;

        _rb = GetComponent<Rigidbody2D>();

        //_moveSuppoter = new GameObject("MoveSupporter");
    }

    private void Update()
    {
        if (HasStateAuthority == false) return;
        InputPlayer();
    
    }
    public override void FixedUpdateNetwork()
    {
        if (HasStateAuthority == false) return;
        MovePlayer();
    }

    private void InputPlayer()
    {
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }


    /// <summary>
    /// 플레이어 이동
    /// </summary>
    void MovePlayer()
    {
        _rb.linearVelocity = Vector2.zero;
        if(_moveDir == Vector2.zero) return;

        Vector2 moveDir = transform.right * _moveDir.x + transform.up * _moveDir.y;
        moveDir.Normalize();
        _rb.linearVelocity = moveDir * _moveSpeed;
    }

}
