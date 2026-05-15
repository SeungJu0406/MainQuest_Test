using UnityEngine;

public class CameraArm : MonoBehaviour
{
    [SerializeField] private float _mouseSensitivity = 2f;
    [SerializeField] private float _pitchMin = -30f;  // 위로 올릴 수 있는 최대 각도
    [SerializeField] private float _pitchMax = 60f;   // 아래로 내릴 수 있는 최대 각도

    private float _yaw;    // 수평 회전 (Y축)
    private float _pitch;  // 수직 회전 (X축)
    private bool _isActive;

    // 로컬 플레이어 스폰 시 호출 → 마우스 입력 활성화
    public void Activate()
    {
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

        // Arm 자체 회전 (Pitch + Yaw) → 자식 CinemachineCamera가 따라서 공전
        transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    // 수평 회전값만 외부에서 읽을 때 사용 (이동 방향, 캐릭터 회전 계산용)
    public float Yaw => _yaw;
}
