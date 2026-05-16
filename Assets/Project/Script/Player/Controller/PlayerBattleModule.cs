using UnityEngine;

[System.Serializable]
public class PlayerBattleModule
{
    private PlayerController _controller;

    public PlayerBattleModule(PlayerController controller)
    {
        _controller = controller;
    }
}
