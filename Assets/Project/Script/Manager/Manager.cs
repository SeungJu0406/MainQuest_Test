using System;

public static class Manager
{
    public static RunnerEventOriginator RunnerEventOriginator { get; private set; }
    public static event Action<RunnerEventOriginator> OnRunnerEventOriginatorSet;


    public static void SetRunnerEventOriginator(RunnerEventOriginator runnerEventOriginator)
    {
        RunnerEventOriginator = runnerEventOriginator;

        if (OnRunnerEventOriginatorSet != null)
            OnRunnerEventOriginatorSet?.Invoke(runnerEventOriginator);
    }
}
