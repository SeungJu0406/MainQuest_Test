using Fusion;
using System;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    public int ColorIndex;

    [SerializeField] private ColorPalette _colorPalette;
    [SerializeField] private float _moveSpeed = 5f;

    private Vector2 _moveDir;
    private Rigidbody2D _rb;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (!HasStateAuthority) return;
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        _rb.linearVelocity = _moveDir * _moveSpeed;

    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    internal void RPC_SetColor(int colorIndex)
    {
        Renderer renderer = GetComponent<Renderer>();
        renderer.material.color = _colorPalette.Colors[colorIndex];
    }
}