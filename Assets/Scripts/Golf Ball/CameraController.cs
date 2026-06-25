using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static Action OnTransitionFinished;
    public enum CameraState { GolfBall, MudMonster, Transition }

    private CameraState _currentState = CameraState.MudMonster;

    [Header("Hierarchy Link")]
    [SerializeField] private Transform _cameraChildTransform;

    [Header("Targets")]
    [SerializeField] private Transform _golfBallTarget;
    [SerializeField] private Camera _mudMonsterCamera;

    [Header("Orbit Settings")]
    [SerializeField] private float _baseDistance;
    [SerializeField] private float _height;
    [SerializeField] private float _orbitSpeed;
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;
    [SerializeField] private float _defaultFOV;

    [Header("Smoothing")]
    [SerializeField] private float _positionSmooth;

    [Header("Transitions")]
    [SerializeField] private float _transitionSpeed;
    [SerializeField] private float _launchYawLerpSpeed;

    [Header("Aim Distance")]
    [SerializeField, Range(1f, 2f)] private float _maxDistanceMultiplier;
    [SerializeField] private float _distanceLerpSpeed;

    [Header("Aim Camera Shake")]
    [SerializeField] private bool _useCameraShake = true;
    [SerializeField] private float _maxShakeFrequency;
    [SerializeField] private float _maxShakeMagnitude;

    private float _yaw;
    private float _pitch = 20f;
    private Vector3 _posVelocity;
    private Camera _camera;

    private bool _lerpingToTargetYaw;
    private float _targetYaw;

    private float _currentDistanceMultiplier = 1f;
    private float _targetDistanceMultiplier = 1f;
    private float _currentAimPower;
    private float _shakeTimeAccumulator;

    private InputSystem_Actions _controls;
    private Vector2 _lookInput;

    private CameraState _targetState;

    void Awake()
    {
        _camera = _cameraChildTransform.GetComponent<Camera>();
    }

    public void InitializeDevice(InputDevice assignedDevice)
    {
        _controls = new InputSystem_Actions();

        if (assignedDevice != null)
        {
            _controls.devices = new[] { assignedDevice };
        }

        _controls.Golf.Look.performed += OnLookPerformed;
        _controls.Golf.Look.canceled += OnLookCanceled;

        _controls.Golf.Enable();
    }

    void OnEnable()
    {
        if (_controls != null) _controls.Golf.Enable();
    }

    void OnDisable()
    {
        if (_controls != null) _controls.Golf.Disable();
    }

    private void OnLookPerformed(InputAction.CallbackContext ctx)
    {
        if (_currentState != CameraState.GolfBall) return;

        _lookInput = ctx.ReadValue<Vector2>();
        if (_lerpingToTargetYaw) _lerpingToTargetYaw = false;
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx) => _lookInput = Vector2.zero;

    void LateUpdate()
    {
        switch (_currentState)
        {
            case CameraState.GolfBall:
                HandleOrbit();
                HandleDistanceMultiplier();
                ApplyGolfBallPosition();
                break;

            case CameraState.MudMonster:
                ApplyMudMonsterPosition();
                break;

            case CameraState.Transition:
                HandleStateTransition();
                break;
        }
    }

    void HandleOrbit()
    {
        _yaw += _lookInput.x * _orbitSpeed * Time.deltaTime;
        _pitch += _lookInput.y * _orbitSpeed * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);

        if (_lerpingToTargetYaw)
        {
            _yaw = Mathf.LerpAngle(_yaw, _targetYaw, _launchYawLerpSpeed * Time.deltaTime);
            if (Mathf.Abs(Mathf.DeltaAngle(_yaw, _targetYaw)) <= 0.05f)
            {
                _yaw = _targetYaw;
                _lerpingToTargetYaw = false;
            }
        }
    }

    void HandleDistanceMultiplier()
    {
        _currentDistanceMultiplier = Mathf.Lerp(
            _currentDistanceMultiplier,
            _targetDistanceMultiplier,
            _distanceLerpSpeed * Time.deltaTime);
    }

    void ApplyGolfBallPosition()
    {
        Vector3 desiredPos = CalculateGolfBallPosition();

        transform.position = Vector3.SmoothDamp(
            transform.position, desiredPos, ref _posVelocity, _positionSmooth);

        transform.LookAt(_golfBallTarget.position);

        if (_useCameraShake && _currentAimPower > 0.01f)
        {
            float currentFrequency = _maxShakeFrequency * _currentAimPower;
            _shakeTimeAccumulator += Time.deltaTime * currentFrequency;

            _cameraChildTransform.localPosition = GetPerlinShakeOffset(_shakeTimeAccumulator);
        }
        else
        {
            _shakeTimeAccumulator = 0f;
            _cameraChildTransform.localPosition = Vector3.zero;
        }

        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, _defaultFOV, _transitionSpeed * Time.deltaTime);
    }

    void ApplyMudMonsterPosition()
    {
        transform.position = _mudMonsterCamera.transform.position;
        transform.rotation = _mudMonsterCamera.transform.rotation;

        _cameraChildTransform.localPosition = Vector3.zero;
        _cameraChildTransform.localRotation = Quaternion.identity;

        _camera.fieldOfView = _mudMonsterCamera.fieldOfView;
    }

    void HandleStateTransition()
    {
        _cameraChildTransform.localPosition = Vector3.zero;

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
            targetPosition = CalculateGolfBallPosition();
            targetRotation = Quaternion.LookRotation(_golfBallTarget.position - targetPosition);
            targetFOV = _defaultFOV;
        }

        transform.position = Vector3.Lerp(transform.position, targetPosition, _transitionSpeed * Time.deltaTime);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, _transitionSpeed * Time.deltaTime);
        _camera.fieldOfView = Mathf.Lerp(_camera.fieldOfView, targetFOV, _transitionSpeed * Time.deltaTime);

        CheckTransitionFinished(targetPosition, targetRotation);
    }

    Vector3 CalculateGolfBallPosition()
    {
        float currentDistance = _baseDistance * _currentDistanceMultiplier;
        Quaternion rot = Quaternion.Euler(_pitch, _yaw, 0f);
        return _golfBallTarget.position + rot * new Vector3(0f, 0f, -currentDistance) + Vector3.up * _height;
    }

    private Vector3 GetPerlinShakeOffset(float shakeTime)
    {
        float noiseX = Mathf.PerlinNoise(shakeTime, 0.0f) * 2f - 1f;
        float noiseY = Mathf.PerlinNoise(0.0f, shakeTime) * 2f - 1f;

        float powerCurve = _currentAimPower * _currentAimPower;
        float intensity = _maxShakeMagnitude * powerCurve;

        return (Vector3.right * noiseX + Vector3.up * noiseY) * intensity;
    }

    private void CheckTransitionFinished(Vector3 targetPosition, Quaternion targetRotation)
    {
        if (Vector3.Distance(transform.position, targetPosition) <= 0.01f &&
            Quaternion.Angle(transform.rotation, targetRotation) <= 0.05f)
        {
            _currentState = _targetState;

            if (_currentState == CameraState.GolfBall)
            {
                _yaw = transform.eulerAngles.y;
                float pitch = transform.eulerAngles.x;
                _pitch = (pitch > 180) ? pitch - 360 : pitch;
            }

            OnTransitionFinished?.Invoke();
        }
    }

    public void ChangeCameraMode(CameraState newState)
    {
        if (newState == _currentState) return;
        _targetState = newState;
        _currentState = CameraState.Transition;
    }

    public void OnBallLaunched(Vector3 launchDir)
    {
        _lerpingToTargetYaw = true;
        _targetYaw = Mathf.Atan2(launchDir.x, launchDir.z) * Mathf.Rad2Deg;
    }

    public void SetAimPower(float power)
    {
        _currentAimPower = Mathf.Clamp01(power);
        _targetDistanceMultiplier = 1f + _currentAimPower * (_maxDistanceMultiplier - 1f);
    }

    public float GetYaw() => _yaw;
}