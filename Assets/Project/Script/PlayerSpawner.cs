using UnityEngine;
using Fusion;

public class PlayerSpawner : SimulationBehaviour, IPlayerJoined, IPlayerLeft
{
    [SerializeField] private PlayerController _playerPrefab;

    private PlayerController _player;

    public void PlayerJoined(PlayerRef player)
    {
        if(player == Runner.LocalPlayer)
        {
            _player= Runner.Spawn(_playerPrefab, Vector3.zero, Quaternion.identity);
        }
    }

    public void PlayerLeft(PlayerRef player)
    {
      
    }
}
