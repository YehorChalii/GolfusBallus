using System;
using UnityEngine;

public static class MudEvents
{
    // Actions
    public static event Action OnMudMonsterShot;
    public static event Action OnMudPuddleLand;

    // Publishers
    public static void RaiseMudMonsterShot() => OnMudMonsterShot?.Invoke();
    public static void RaiseMudPuddleLand() => OnMudPuddleLand?.Invoke();
}
