using Fusion;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private PlayerController _playerPrefab;

    [Networked, Capacity(8)]
    public NetworkLinkedList<int> AvailableColorIndex { get; }

    private void Awake()
    {
        // 이벤트 구독은 Awake에서 (Runner 설정 전에 등록 필요)
        Manager.OnRunnerEventOriginatorSet += OnRunnerEventOriginatorSet;
    }

    private void OnDestroy()
    {
        Manager.SetPlayerSpawner(null);
        Manager.OnRunnerEventOriginatorSet -= OnRunnerEventOriginatorSet;
    }

    public bool IsSpawnedReady { get; private set; }

    public override void Spawned()
    {
        // Fusion 초기화 완료 후 등록 → 항상 유효한 인스턴스만 Manager에 세팅
        Manager.SetPlayerSpawner(this);
        IsSpawnedReady = true;

        if (!HasStateAuthority) return;

        for (int i = 0; i < 4; i++)
            AvailableColorIndex.Add(i);
    }

    private void OnRunnerEventOriginatorSet(RunnerEventOriginator originator)
    {
        if (originator == null) return;
        originator.OnPlayerJoined += PlayerJoined;
        originator.OnPlayerLeft += PlayerLeft;
    }

    private void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player != runner.LocalPlayer) return;

        runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity);
    }

    private void PlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (!HasStateAuthority) return;

        foreach (var netObj in runner.GetAllNetworkObjects())
        {
            var ctrl = netObj.GetComponent<PlayerController>();
            if (ctrl == null || ctrl.Owner != player) continue;
            runner.Despawn(netObj);
            break;
        }
    }

    public void ReturnColor(int colorIndex)
    {
        AvailableColorIndex.Add(colorIndex);
    }

    [Rpc(RpcSources.All, RpcTargets.StateAuthority)]
    public void RPC_DequeueColor(int colorIndex)
    {
        AvailableColorIndex.Remove(colorIndex);
    }
}
