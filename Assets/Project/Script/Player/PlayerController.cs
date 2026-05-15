using Fusion;
using UnityEngine;

public class PlayerController : NetworkBehaviour
{
    [Networked] public PlayerRef Owner { get; set; }
    [Networked] public int ColorIndex { get; set; }

    [SerializeField] private ColorPalette _colorPalette;
    [SerializeField] private float _moveSpeed = 5f;

    private Vector2 _moveDir;
    private Rigidbody2D _rb;
    private ChangeDetector _changes;
    private bool _colorRequested;

    public override void Spawned()
    {
        _rb = GetComponent<Rigidbody2D>();
        _changes = GetChangeDetector(ChangeDetector.Source.SimulationState);

        if (HasStateAuthority)
            Owner = Runner.LocalPlayer;
    }

    private void Update()
    {
        if (!HasStateAuthority) return;
        _moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")).normalized;
    }

    public override void FixedUpdateNetwork()
    {
        if (!HasStateAuthority) return;

        if (!_colorRequested)
        {
            _colorRequested = true;
            var spawner = Manager.PlayerSpawner;
            if (spawner != null && spawner.AvailableColorIndex.Count > 0)
            {
                int colorIndex = -1;
                foreach (var idx in spawner.AvailableColorIndex)
                    colorIndex = idx;

                ColorIndex = colorIndex;                  // 본인 StateAuthority라 직접 세팅
                spawner.RPC_DequeueColor(colorIndex);     // MasterClient에 삭제 요청
            }
        }

        _rb.linearVelocity = _moveDir * _moveSpeed;
    }

    public override void Render()
    {
        foreach (var change in _changes.DetectChanges(this, out _, out _))
        {
            if (change == nameof(ColorIndex))
                ApplyColor(ColorIndex);
        }
    }

    private void ApplyColor(int index)
    {
        if (_colorPalette == null || index < 0 || index >= _colorPalette.Colors.Length) return;
        var rend = GetComponent<Renderer>();
        if (rend) rend.material.color = _colorPalette.Colors[index];
    }
}