using UnityEngine;

public class PlayerPhysicController : MonoBehaviour
{
    public bool IsMyPlayer; 

   [SerializeField] private  float _moveSpeed; 

    Vector3 _inputDir;
    Rigidbody2D _rb; 

    private void Awake()
    {
        Init();
    }

    private void Update()
    {
        if (IsMyPlayer == false) return;
        InputPlayer();

    }

    private void FixedUpdate()
    {
        if (IsMyPlayer == false) return;

        Move();
        Rotate();
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (IsMyPlayer == false) return;
    }

    void InputPlayer()
    {
        _inputDir.x = Input.GetAxisRaw("Horizontal");
        _inputDir.z = Input.GetAxisRaw("Vertical");
    }

    public void Move()
    {
        _rb.linearVelocity = Vector2.zero;
        if (_inputDir == Vector3.zero)
        {
            return;
        }

        Debug.Log(_inputDir);
        Vector3 moveDir = transform.right * _inputDir.x + transform.up * _inputDir.z;
        moveDir.Normalize();
        _rb.linearVelocity = moveDir * _moveSpeed;
    }

    public void Rotate()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        transform.rotation = Quaternion.Euler(
            transform.eulerAngles.x,
            Camera.main.transform.eulerAngles.y,
            transform.eulerAngles.z);
    }

    //---------------------------------------------------------------

    void Init()
    {
        InitGetComponent();
    }


    /// <summary>
    /// GetComponent �ʱ�ȭ
    /// </summary>
    void InitGetComponent()
    {
        _rb = GetComponent<Rigidbody2D>();
    }
}
