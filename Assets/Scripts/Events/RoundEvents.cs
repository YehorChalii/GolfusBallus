using System;
using UnityEngine;

public static class RoundEvents
{
    public enum WinnerType { GolfBall, MudMonster }

    // State Changes
    public static event Action OnRainSetupComplete;
    public static event Action<Vector3> OnWindConfirmed;
    public static event Action<WinnerType> OnGameOver;

    // Publishers
    public static void RaiseRainSetupComplete() => OnRainSetupComplete?.Invoke();
    public static void RaiseWindConfirmed(Vector3 direction) => OnWindConfirmed?.Invoke(direction);
    public static void RaiseGameOver(WinnerType winner) => OnGameOver?.Invoke(winner);
}