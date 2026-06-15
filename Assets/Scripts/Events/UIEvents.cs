using System;
using UnityEngine;

public static class UIEvents
{
    // Generic UI
    public static event Action OnGenericButtonPressed;

    // Specific Flow UI
    public static event Action<Vector3> OnWindDirectionConfirmed;
    public static event Action OnRestartRequested;
    public static event Action OnJoinMenuCompleted;

    // Publishers
    public static void RaiseGenericButtonPressed() => OnGenericButtonPressed?.Invoke();
    public static void RaiseWindDirectionConfirmed(Vector3 windDirection) => OnWindDirectionConfirmed?.Invoke(windDirection);
    public static void RaiseRestartRequested() => OnRestartRequested?.Invoke();
    public static void RaiseJoinMenuCompleted() => OnJoinMenuCompleted?.Invoke();
}