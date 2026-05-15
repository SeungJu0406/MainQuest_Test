using Fusion;
using Unity.Cinemachine;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private ColorPalette _colorPalette;
    [SerializeField] private float _moveSpeed = 5f;

    private Rigidbody _rb;
    private Animator _animator;
    private ChangeDetector _changes;
    private Vector3 _moveDir;
    private bool _colorRequested;
    private Camera _cam;

    public override void Spawned()
    {
        ApplyColor(ColorIndex);

        _rb = GetComponent<Rigidbody>();
        _animator = GetComponentInChildren<Animator>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
        {
            Owner = Runner.LocalPlayer;
            // 본인 클라이언트의 VirtualCamera만 활성화 (다른 플레이어 카메라는 비활성 유지)
            var vcam = GetComponentInChildren<CinemachineCamera>();
            if (vcam != null) vcam.gameObject.SetActive(true);
            _cam = Camera.main;
        }
    }

    public override void Despawned(NetworkRunner runner, bool hasState)
    {
        if (!hasState) return;
        var spawner = Manager.PlayerSpawner;
        if (spawner == null) return;
        spawner.RPC_EnqueueColor(ColorIndex);
    }

    private void Update()
    {
        if (!HasStateAuthority) return;

        // 카메라 forward/right를 수평면(Y=0)에 투영해 이동 방향 계산
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        Vector3 camForward = Vector3.ProjectOnPlane(_cam.transform.forward, Vector3.up).normalized;
        Vector3 camRight   = Vector3.ProjectOnPlane(_cam.transform.right,   Vector3.up).normalized;
        _moveDir = (camForward * v + camRight * h).normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (!_colorRequested)
        {
            var spawner = Manager.PlayerSpawner;
            if (spawner == null || !spawner.IsSpawnedReady || spawner.AvailableColorIndex.Count == 0)
                return;

            _colorRequested = true;
            int colorIndex = spawner.AvailableColorIndex[spawner.AvailableColorIndex.Count - 1];
            ColorIndex = colorIndex;
            spawner.RPC_DequeueColor(colorIndex);
        }

        // Y축(중력)은 유지하고 XZ만 이동 제어
        _rb.linearVelocity = new Vector3(
            _moveDir.x * _moveSpeed,
            _rb.linearVelocity.y,
            _moveDir.z * _moveSpeed
        );

        // 이동 방향으로 캐릭터 회전
        if (_moveDir != Vector3.zero)
            transform.rotation = Quaternion.LookRotation(_moveDir);
    }

    public override void Render()
    {
        // 수평 속도 크기로 애니메이터 Speed 파라미터 갱신 (모든 클라이언트에서 실행)
        if (_animator != null)
        {
            float speed = new Vector2(_rb.linearVelocity.x, _rb.linearVelocity.z).magnitude;
            _animator.SetFloat("Speed", speed);
        }

        foreach (var change in _changes.DetectChanges(this, out _, out _))
        {
            if (change == nameof(ColorIndex))
                ApplyColor(ColorIndex);
        }
    }

    private void ApplyColor(int index)
    {
        if (_colorPalette == null || index < 0 || index >= _colorPalette.Colors.Length) return;
        // Y봇 스킨메시는 자식에 있으므로 GetComponentInChildren 사용
        var rend = GetComponentInChildren<Renderer>();
        if (rend) rend.material.color = _colorPalette.Colors[index];
    }
}
