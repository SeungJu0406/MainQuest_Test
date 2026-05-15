using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _pitchMin = -30f;  // 위로 올릴 수 있는 최대 각도
    [SerializeField] private float _pitchMax = 60f;   // 아래로 내릴 수 있는 최대 각도

    private float _yaw;
    private float _pitch;
    private bool _isActive;
    private Transform _followTarget;  // 플레이어 Transform (계층 분리 후 추적용)

    // 로컬 플레이어 스폰 시 호출
    public void Activate()
    {
        // 플레이어 계층에서 분리 → 물리 충돌 보정값이 카메라에 전달되지 않음
        _followTarget = transform.parent;
        transform.SetParent(null);

        _isActive = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        if (!_isActive) return;

        _yaw   += Input.GetAxisRaw("Mouse X") * _mouseSensitivity;
        _pitch -= Input.GetAxisRaw("Mouse Y") * _mouseSensitivity;  // 반전: 마우스 위 → 카메라 위
        _pitch  = Mathf.Clamp(_pitch, _pitchMin, _pitchMax);
    }

    private void LateUpdate()
    {
        if (!_isActive) return;

        // 물리 처리 완료 후 플레이어 위치 추적 → 충돌 보정 전 위치가 아닌 최종 위치 기준
        if (_followTarget != null)
            transform.position = _followTarget.position;

        // Arm 회전 (Pitch + Yaw) → 자식 CinemachineCamera가 따라서 공전
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private void OnDestroy()
    {
        // 씬 정리 시 분리된 오브젝트 제거
        if (_followTarget == null)
            Destroy(gameObject);
    }

    // 수평 회전값만 외부에서 읽을 때 사용 (이동 방향, 캐릭터 회전 계산용)
    public float Yaw => _yaw;
}
