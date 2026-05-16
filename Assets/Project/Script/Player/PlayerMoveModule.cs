using UnityEngine;

[System.Serializable]
public class PlayerMoveModule
{
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody2D _rb;
    private Vector2 _moveDir;

    public int FacingDirX { get; private set; } = 1;

    public void Init(Rigidbody2D rb)
    {
        _rb = rb;
    }

    public void ReadInput()
    {
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
        if (_moveDir.x != 0)
            FacingDirX = _moveDir.x > 0 ? 1 : -1;
    }

    public void ApplyMovement()
    {
        _rb.linearVelocity = _moveDir * _moveSpeed;
    }
}
