using System;

public static class Manager
{
    public static RunnerEventOriginator RunnerEventOriginator { get; private set; }
    public static event Action<RunnerEventOriginator> OnRunnerEventOriginatorSet;

    public static PlayerSpawner PlayerSpawner { get; private set; }
    public static GameManager GameManager { get; private set; }

    public static void SetRunnerEventOriginator(RunnerEventOriginator runnerEventOriginator)
    {
        RunnerEventOriginator = runnerEventOriginator;
        OnRunnerEventOriginatorSet?.Invoke(runnerEventOriginator);
    }

    public static void SetPlayerSpawner(PlayerSpawner playerSpawner)
    {
        PlayerSpawner = playerSpawner;
    }

    public static void SetGameManager(GameManager gameManager)
    {
        GameManager = gameManager;
    }
}
