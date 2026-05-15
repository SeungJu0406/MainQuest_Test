using Fusion;
using System;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private float _moveSpeed = 5f; // 이동 속도

    Vector2 _moveDir;
    Rigidbody2D _rb;
    public override void Spawned()
    {
        if (HasStateAuthority == false) return;

        _rb = GetComponent<Rigidbody2D>();
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
        //transform.Translate(_moveDir * _moveSpeed * Time.fixedDeltaTime);
        _rb.linearVelocity = new Vector2(_moveDir.x * _moveSpeed, _moveDir.y * _moveSpeed);
    }

}
