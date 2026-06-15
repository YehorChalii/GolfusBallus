using UnityEngine;
using UnityEngine.InputSystem;

public class GolfBallCameraController : MonoBehaviour
{
    [Header("Hardware Links")]
    [SerializeField] private Transform _target;
    [SerializeField] private CameraOrbit _cameraOrbit;
    [SerializeField] private CameraShake _cameraShake;

    [Header("Gameplay Camera Tuning")]
    [SerializeField, Range(1f, 2f)] private float _maxDistanceMultiplier = 1.4f;
    [SerializeField] private float _distanceMultiplierLerpSpeed = 4f;
    [SerializeField] private float _toLaunchDirectionLerpSpeed = 6f;

    private InputSystem_Actions _controls;
    private Vector2 _lookInput;
    private bool _isActiveTracking;

    private float _currentAimPower;
    private float _currentDistanceModifier = 1f;
    private float _targetDistanceModifier = 1f;

    public void InitializeDevice(InputDevice assignedDevice)
    {
        _controls = new InputSystem_Actions();
        if (assignedDevice != null) _controls.devices = new[] { assignedDevice };

        _controls.Golf.Look.performed += ctx => _lookInput = ctx.ReadValue<Vector2>();
        _controls.Golf.Look.canceled += ctx => _lookInput = Vector2.zero;
        _controls.Golf.Enable();
    }

    public void SetTrackingActive(bool active)
    {
        _isActiveTracking = active;
        if (active) _cameraOrbit.Setup(_target);
    }

    private void OnEnable()
    {
        if (_controls != null) _controls.Golf.Enable();

        GolfBallEvents.OnGolfBallAimStarted += HandleAimStarted;
        GolfBallEvents.OnGolfBallLaunched += HandleLaunch;
    }

    private void OnDisable()
    {
        if (_controls != null) _controls.Golf.Disable();

        GolfBallEvents.OnGolfBallAimStarted -= HandleAimStarted;
        GolfBallEvents.OnGolfBallLaunched -= HandleLaunch;
    }

    private void HandleAimStarted() => _currentAimPower = 0.02f;

    public void UpdateAimPowerValues(float normalizedPower)
    {
        _currentAimPower = normalizedPower;
        _targetDistanceModifier = 1f + _currentAimPower * (_maxDistanceMultiplier - 1f);
    }

    private void HandleLaunch(Vector3 launchDir)
    {
        _currentAimPower = 0f;
        _targetDistanceModifier = 1f;

        float targetYaw = Mathf.Atan2(launchDir.x, launchDir.z) * Mathf.Rad2Deg;
        _cameraOrbit.OverrideYawAngle(targetYaw, _toLaunchDirectionLerpSpeed);
    }

    void LateUpdate()
    {
        if (!_isActiveTracking) return;

        _cameraOrbit.ProcessOrbitInput(_lookInput);

        _currentDistanceModifier = Mathf.Lerp(_currentDistanceModifier, _targetDistanceModifier, _distanceMultiplierLerpSpeed * Time.deltaTime);
        _cameraOrbit.SetDistanceMultiplier(_currentDistanceModifier);

        _cameraOrbit.ExecuteUpdate();

        if (_cameraShake != null)
        {
            _cameraShake.ProcessShake(_currentAimPower);
        }
    }
}