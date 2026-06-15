using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GolfBallController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraController _cameraController;

    [Header("Launch Settings")]
    [SerializeField] private float _maxLaunchForce;
    [SerializeField] private float _aimDeadzone;

    private float _currentSpeed;
    private float _normalizedCurrentSpeed;

    [Header("Ball States")]
    [SerializeField] private float _stopSpeedThreshold;
    [SerializeField] private float _afterLaunchCheckDelay;
    [SerializeField] private float _afterStopDelay;
    [SerializeField, Range(0.9f, 1f)] private float rollingFriction;
    [SerializeField, Range(0.8f, 1f)] private float stoppingFriction;

    private float _launchTimer;

    [Header("Mud")]
    [SerializeField] private LayerMask _mudLayer;
    [SerializeField] private LayerMask _mudMonsterLayer;
    [SerializeField, Range(0f, 1f)] private float _mudFrictionMultiplier;

    private bool _isInMud;

    [Header("Wind")]
    [SerializeField] private float _maxWindForce;
    [SerializeField, Range(0f, 1f)] private float _windAlignmentForceMultiplier;

    private Vector3 _windDirection;

    [Header("Input")]
    [SerializeField] private float _aimSmoothTime;

    private Vector2 _aimInput;
    private Vector2 _smoothedAimInput;
    private Vector2 _aimSmoothVelocity;
    private Vector2 _currentPull;

    private enum BallState { Idle, Aiming, Launched }
    private BallState _state = BallState.Idle;

    private Rigidbody _rb;
    private InputSystem_Actions _controls;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    public void InitializeDevice(InputDevice assignedDevice)
    {
        _controls = new InputSystem_Actions();

        if (assignedDevice != null)
        {
            _controls.devices = new[] { assignedDevice };
        }

        _controls.Golf.Launch.performed += OnR2Pressed;
        _controls.Golf.Aim.performed += ctx => _aimInput = ctx.ReadValue<Vector2>();
        _controls.Golf.Aim.canceled += ctx => _aimInput = Vector2.zero;

        _controls.Golf.Enable();
    }

    void OnEnable() => _controls?.Golf.Enable();
    void OnDisable() => _controls?.Golf.Disable();

    void OnR2Pressed(InputAction.CallbackContext ctx)
    {
        if (_state != BallState.Aiming) return;

        if (_currentPull.magnitude < _aimDeadzone)
        {
            CancelAim();
            return;
        }

        Launch();
    }

    void Launch()
    {
        _state = BallState.Launched;
        _launchTimer = 0f;

        Vector3 launchDirection = GetLaunchDirection();
        float normalizedPower = GetNormalizedPower();
        float launchForce = normalizedPower * _maxLaunchForce;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.AddForce(launchDirection * launchForce, ForceMode.Impulse);

        GolfBallEvents.RaiseGolfBallLaunched(launchDirection);

        _currentPull = Vector2.zero;
    }

    private float GetNormalizedPower()
    {
        return Mathf.Clamp01(_currentPull.magnitude);
    }

    void CancelAim()
    {
        _state = BallState.Idle;
        _currentPull = Vector2.zero;
        GolfBallEvents.RaiseGolfBallAimCanceled();
    }

    void Update()
    {
        SmoothAimInput();

        if (_state == BallState.Aiming)
        {
            _currentPull = GetCameraRelativeInput();
        }

        HandleAimState();
    }

    private void SmoothAimInput()
    {
        _smoothedAimInput = Vector2.SmoothDamp(_smoothedAimInput, _aimInput, ref _aimSmoothVelocity, _aimSmoothTime);
    }

    void HandleAimState()
    {
        if (_state == BallState.Launched)
        {
            _launchTimer += Time.deltaTime;
            return;
        }

        bool joystickPulled = _smoothedAimInput.magnitude >= _aimDeadzone;

        if (joystickPulled && _state == BallState.Idle)
        {
            _state = BallState.Aiming;
            GolfBallEvents.RaiseGolfBallAimStarted();
        }
        else if (!joystickPulled && _state == BallState.Aiming)
        {
            CancelAim();
            return;
        }

        if (_state == BallState.Aiming)
        {
            Vector3 launchDirection = GetLaunchDirection();
            float normalizedPower = GetNormalizedPower();

            GolfBallEvents.RaiseGolfBallAimUpdated(launchDirection, normalizedPower);
        }
    }

    void FixedUpdate()
    {
        if (_state == BallState.Launched)
        {
            SetCurrentSpeed();
            ApplyFriction();
            ApplyWindForce();

            if (_launchTimer > _afterLaunchCheckDelay)
            {
                CheckStop();
            }

            CheckOutOfMap();
        }
    }

    void SetCurrentSpeed()
    {
        _currentSpeed = _rb.linearVelocity.magnitude;
        _normalizedCurrentSpeed = Mathf.Clamp01(_currentSpeed / _maxLaunchForce);
    }

    void ApplyFriction()
    {
        float friction = Mathf.Lerp(stoppingFriction, rollingFriction, _normalizedCurrentSpeed);

        if (_isInMud)
        {
            friction *= _mudFrictionMultiplier;
        }

        _rb.linearVelocity *= friction;
    }

    void ApplyWindForce()
    {
        if (_windDirection.sqrMagnitude > 0f && _currentSpeed > 0.001f)
        {
            float dot = Vector3.Dot(_rb.linearVelocity.normalized, _windDirection.normalized);
            float normalizedDotValue = (dot + 1f) / 2f;
            float windAlignmentMultiplier = Mathf.Lerp(1f, _windAlignmentForceMultiplier, normalizedDotValue);
            float finalForce = _maxWindForce * windAlignmentMultiplier * _normalizedCurrentSpeed;

            Vector3 windForce = _windDirection.normalized * finalForce;
            _rb.AddForce(windForce, ForceMode.Force);
        }
    }

    void CheckStop()
    {
        if (_rb.linearVelocity.magnitude > _stopSpeedThreshold) return;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;

        if (_isInMud && _state == BallState.Launched)
        {
            OnLose();
            return;
        }

        if (_state == BallState.Launched)
        {
            _state = BallState.Idle;
            StartCoroutine(DelayStopEvent());
        }
    }

    private IEnumerator DelayStopEvent()
    {
        yield return new WaitForSeconds(_afterStopDelay);
        GolfBallEvents.RaiseGolfBallStopped();
    }

    void CheckOutOfMap()
    {
        if (transform.position.y < -1f)
        {
            OnLose();
        }
    }

    void OnLose()
    {
        _state = BallState.Idle;
        RoundEvents.RaiseGameOver(RoundEvents.WinnerType.MudMonster);
        Stop();
    }

    void Stop()
    {
        if (_rb.isKinematic) return;

        _rb.linearVelocity = Vector3.zero;
        _rb.angularVelocity = Vector3.zero;
        _rb.isKinematic = true;
    }

    public void UpdateWindDirection(Vector3 windDirection)
    {
        _windDirection = windDirection;
    }

    private void OnTriggerEnter(Collider other)
    {
        if ((_mudLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _isInMud = true;
            GolfBallEvents.RaiseGolfBallMudEnter();
        }

        if ((_mudMonsterLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            RoundEvents.RaiseGameOver(RoundEvents.WinnerType.GolfBall);
            Stop();
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if ((_mudLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            _isInMud = false;
            GolfBallEvents.RaiseGolfBallMudExit();
        }
    }

    Vector2 GetCameraRelativeInput()
    {
        float yawRad = _cameraController.GetYaw() * Mathf.Deg2Rad;

        Vector3 camForward = new Vector3(Mathf.Sin(yawRad), 0f, Mathf.Cos(yawRad));
        Vector3 camRight = new Vector3(Mathf.Cos(yawRad), 0f, -Mathf.Sin(yawRad));

        Vector3 worldPull = camRight * _smoothedAimInput.x + camForward * _smoothedAimInput.y;
        return new Vector2(worldPull.x, worldPull.z);
    }

    Vector3 GetLaunchDirection()
    {
        return new Vector3(_currentPull.x, 0f, _currentPull.y).normalized;
    }
}