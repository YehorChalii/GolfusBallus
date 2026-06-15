using UnityEngine;
using System;

public class AimBarUIManager : MonoBehaviour
{
    public Action<float, float> OnAimSequenceFinished;

    [SerializeField] private AimBarUI _firstBar;
    [SerializeField] private AimBarUI _secondBar;

    public enum State { Bar1, Bar2, Finished }
    public State CurrentState { get; private set; }

    void Start() => ResetSystem();

    public void ProgressState()
    {
        if (CurrentState == State.Bar1)
        {
            _firstBar.SetActive(false);
            _secondBar.SetActive(true);
            CurrentState = State.Bar2;
        }
        else if (CurrentState == State.Bar2)
        {
            _secondBar.SetActive(false);
            CurrentState = State.Finished;

            float horizontalForce = GetNormalized(AimBarUI.AXIS.Horizontal);
            float verticalForce = GetNormalized(AimBarUI.AXIS.Vertical);

            OnAimSequenceFinished?.Invoke(horizontalForce, verticalForce);
        }
    }

    public float GetNormalized(AimBarUI.AXIS axis)
    {
        return axis switch
        {
            AimBarUI.AXIS.Horizontal => _firstBar.Value01,
            AimBarUI.AXIS.Vertical => _secondBar.Value01,
            _ => 0f
        };
    }

    public void ResetSystem()
    {
        _firstBar.ResetBar();
        _secondBar.ResetBar();
        
        _firstBar.SetActive(true);
        _secondBar.SetActive(false);
        CurrentState = State.Bar1;
    }
}
