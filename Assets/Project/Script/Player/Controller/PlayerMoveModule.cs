using UnityEngine;

[System.Serializable]
public class PlayerMoveModule
{
    [SerializeField] private float _moveSpeed = 5f;

    private PlayerController _controller;
    private Rigidbody2D _rb;
    private Vector2 _moveDir { get => _controller.MoveDir; set => _controller.MoveDir = value; }

    public int FacingDirX { get; private set; } = 1;

    public void Init(PlayerController controller, Rigidbody2D rb)
    {
        _controller = controller;
        _rb = rb;
    }

    public void ReadInput()
    {
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (_moveDir.x != 0)
        {
            FacingDirX = _moveDir.x > 0 ? 1 : -1;
            // 바라보는 방향으로 스프라이트 FlipX 처리
            if (FacingDirX != 0)
                _controller.RPC_BroadcastSpriteFlipX(FacingDirX < 0);
        }

    }

    public void ApplyMovement()
    {
        _rb.linearVelocity = _moveDir * _moveSpeed;
    }
}
