using Fusion;
using UnityEngine;

public class PlayerSpawner : NetworkBehaviour
{
    public static PlayerSpawner Instance { get; private set; }

    [SerializeField] private PlayerController _playerPrefab;

    [Networked, Capacity(8)]
    private NetworkLinkedList<int> AvailableColorIndex { get; }

    private void Awake()
    {
        Instance = this;
        Manager.OnRunnerEventOriginatorSet += OnRunnerEventOriginatorSet;
    }

    private void OnDestroy()
    {
        Instance = null;
        Manager.OnRunnerEventOriginatorSet -= OnRunnerEventOriginatorSet;
    }

    public override void Spawned()
    {
        if (!Runner.IsSharedModeMasterClient) return;

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
        if (!runner.IsSharedModeMasterClient) return;

        foreach (var netObj in runner.GetAllNetworkObjects())
        {
            var ctrl = netObj.GetComponent<PlayerController>();
            if (ctrl == null || ctrl.Owner != player) continue;

            AvailableColorIndex.Add(ctrl.ColorIndex);
            runner.Despawn(netObj);
            break;
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    public void RPC_RequestColor(NetworkObject playerObj)
    {
        if (!Runner.IsSharedModeMasterClient) return;
        if (AvailableColorIndex.Count == 0) return;

        int colorIndex = -1;
        foreach (var idx in AvailableColorIndex)
            colorIndex = idx;

        AvailableColorIndex.Remove(colorIndex);
        playerObj.GetComponent<PlayerController>().RPC_SetColor(colorIndex);
    }
}
