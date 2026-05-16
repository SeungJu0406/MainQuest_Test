using UnityEngine;

// O존 또는 X존 발판. 트리거 없이 좌표 범위 판정 전용.
// GameManager가 라운드 종료 시점에 Contains()로 플레이어 위치를 조회한다.
public class OXZone : MonoBehaviour
{
    [SerializeField] private bool _isO;  // true = O존, false = X존

    private Collider2D _col;

    public bool IsO => _isO;

    private void Awake()
    {
        _col = GetComponent<Collider2D>();
    }

    // 주어진 2D 위치가 이 존의 범위 안에 있는지 반환
    public bool Contains(Vector2 position)
    {
        return _col.bounds.Contains(position);
    }
}
