using System;
using Unity.VisualScripting;
using UnityEngine;

public static class GolfBallEvents
{
    // Actions
    public static event Action OnGolfBallAimStarted;
    public static event Action<Vector3, float> OnGolfBallAimUpdated;
    public static event Action OnGolfBallAimCanceled;

    public static event Action<Vector3> OnGolfBallLaunched;
    public static event Action OnGolfBallStopped;

    public static event Action OnGolfBallMudEnter;
    public static event Action OnGolfBallMudExit;

    // Publishers
    public static void RaiseGolfBallAimStarted() => OnGolfBallAimStarted?.Invoke();
    public static void RaiseGolfBallAimUpdated(Vector3 direction, float normalizedPower) => OnGolfBallAimUpdated?.Invoke(direction, normalizedPower);
    public static void RaiseGolfBallAimCanceled() => OnGolfBallAimCanceled?.Invoke();
    public static void RaiseGolfBallLaunched(Vector3 direction) => OnGolfBallLaunched?.Invoke(direction);
    public static void RaiseGolfBallStopped() => OnGolfBallStopped?.Invoke();
    public static void RaiseGolfBallMudEnter() => OnGolfBallMudEnter?.Invoke();
    public static void RaiseGolfBallMudExit() => OnGolfBallMudExit?.Invoke();
}
