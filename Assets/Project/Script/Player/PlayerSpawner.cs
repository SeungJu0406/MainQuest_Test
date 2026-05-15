using Fusion;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using static Unity.Collections.Unicode;

public class PlayerSpawner : NetworkBehaviour
{
    [SerializeField] private PlayerController _playerPrefab;

    [SerializeField]
    [Networked, Capacity(8)] private NetworkLinkedList<int> AvailableColorIndex { get; }


    private PlayerController _player;
    [SerializeField]private Color _playerColor;

    private void Awake()
    {
        Manager.OnRunnerEventOriginatorSet += OnRunnerEventOriginatorSet;
    }

    public override void Spawned()
    {
        if (!Runner.IsSharedModeMasterClient) return;

        // Awake에서 하면 안됨, 여기서 초기화
        AvailableColorIndex.Add(0);
        AvailableColorIndex.Add(1);
        AvailableColorIndex.Add(2);
        AvailableColorIndex.Add(3);
    }

    private void OnDestroy()
    {
        Manager.OnRunnerEventOriginatorSet -= OnRunnerEventOriginatorSet;
    }

    private void OnRunnerEventOriginatorSet(RunnerEventOriginator originator)
    {
        if (originator == null) return;
        originator.OnPlayerJoined += PlayerJoined;
        originator.OnPlayerLeft += PlayerLeft;
    }

    public void PlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            _player = runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity);

            RPC_DequeueColor(_player.Object);
        }
    }

    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_DequeueColor(NetworkObject playerObj)
    {
        if (!Runner.IsSharedModeMasterClient) return;

        int colorIndex = AvailableColorIndex.Count - 1;
        AvailableColorIndex.Remove(colorIndex);
        playerObj.GetComponent<PlayerController>().RPC_SetColor(colorIndex);
    }

    public void PlayerLeft(NetworkRunner runner, PlayerRef player)
    {
        if (player == runner.LocalPlayer)
        {
            int colorIndex = _player.Object.GetComponent<PlayerController>().ColorIndex;
            RPC_EnqueueColor(colorIndex);
            runner.Despawn(_player.Object);
        }
    }
    [Rpc(RpcSources.All, RpcTargets.All)]
    private void RPC_EnqueueColor(int colorIndex)
    {
        if (!Runner.IsSharedModeMasterClient) return;

        AvailableColorIndex.Add(colorIndex);
    }
}
