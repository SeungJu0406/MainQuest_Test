using Fusion;
using UnityEngine;
using UnityEngine.Events;
public class RunnerEventOriginator : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    public UnityAction<NetworkRunner, PlayerRef> OnPlayerJoined;
    public UnityAction<NetworkRunner, PlayerRef> OnPlayerLeft;

    private void Awake()
    {
        Manager.SetRunnerEventOriginator(this);
    }
    private void OnDestroy()
    {
        Manager.SetRunnerEventOriginator(null);
    }

    public void PlayerJoined(PlayerRef player)
    {
        OnPlayerJoined?.Invoke(Runner, player);
    }

    public void PlayerLeft(PlayerRef player)
    {
        OnPlayerLeft?.Invoke(Runner, player);
    }
}
