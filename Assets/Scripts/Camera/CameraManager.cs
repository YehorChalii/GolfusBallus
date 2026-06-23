using System;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public enum CameraState { GolfBall, MudMonster, Transition }
    private CameraState _currentState = CameraState.MudMonster;
    private CameraState _targetState;

    public event Action OnTransitionFinished;

    [Header("Targets")]
    [SerializeField] private GolfBallCameraController _golfBallCamera;
    [SerializeField] private Camera _mudMonsterCamera;

    [Header("Transition Settings")]
    [SerializeField] private float _transitionSpeed;
    [SerializeField] private float _defaultFOV;

    private Camera _mainCamera;

    void Awake()
    {
        _mainCamera = GetComponent<Camera>();
    }

    private void Start()
    {
        SetState(CameraState.MudMonster);
    }

    void LateUpdate()
    {
        switch (_currentState)
        {
            case CameraState.MudMonster:
                SetTargetTransform(_mudMonsterCamera.transform, _mudMonsterCamera.fieldOfView);
                break;

            case CameraState.GolfBall:
                SetTargetTransform(_golfBallCamera.transform, _defaultFOV);
                break;

            case CameraState.Transition:
                ExecuteStateTransition();
                break;
        }
    }

    public void ChangeCameraState(CameraState newState)
    {
        if (newState == _currentState) return;

        _targetState = newState;
        _currentState = CameraState.Transition;

        _golfBallCamera.SetTrackingActive(false);
    }

    private void SetTargetTransform(Transform target, float targetFOV)
    {
        transform.position = target.position;
        transform.rotation = target.rotation;
        _mainCamera.fieldOfView = targetFOV;
    }

    private void ExecuteStateTransition()
    {
        Vector3 targetPosition;
        Quaternion targetRotation;
        float targetFOV;

        if (_targetState == CameraState.MudMonster)
        {
            targetPosition = _mudMonsterCamera.transform.position;
            targetRotation = _mudMonsterCamera.transform.rotation;
            targetFOV = _mudMonsterCamera.fieldOfView;
        }
        else
        {
            targetPosition = _golfBallCamera.transform.position;
            targetRotation = _golfBallCamera.transform.rotation;
            targetFOV = _defaultFOV;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, _transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _transitionSpeed * Time.deltaTime);
        _mainCamera.fieldOfView = Mathf.Lerp(_mainCamera.fieldOfView, targetFOV, _transitionSpeed * Time.deltaTime);

        if (Vector3.Distance(transform.position, targetPosition) <= 0.01f &&
            Quaternion.Angle(transform.rotation, targetRotation) <= 0.05f)
        {
            SetState(_targetState);
            OnTransitionFinished?.Invoke();
        }
    }

    private void SetState(CameraState state)
    {
        _currentState = state;
        if (_currentState == CameraState.GolfBall)
        {
            _golfBallCamera.SetTrackingActive(true);
        }
    }
}