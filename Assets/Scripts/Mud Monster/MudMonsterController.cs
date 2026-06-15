using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MudMonsterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mudProjectile;
    [SerializeField] private Transform _shootTransform;
    [SerializeField] private CameraController _cameraController;
    [SerializeField] private AimBarUIManager _aimBarUIManager;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;

    [Header("Shoot Settings")]
    [SerializeField] private float _fixedAngle;
    [SerializeField] private float _minForce;
    [SerializeField] private float _maxForce;
    [SerializeField] private float _afterShootDelay;

    [Header("Animations")]
    [SerializeField] private Animator _animator;

    private InputSystem_Actions _controls;
    private bool _isAiming = false;

    private float _horizontalValue = 0.5f;
    private float _verticalValue = 0.5f;
    private Vector3 _currentWindDirection;

    public void InitializeDevice(InputDevice assignedDevice)
    {
        _controls = new InputSystem_Actions();

        if (assignedDevice != null)
        {
            _controls.devices = new[] { assignedDevice };
        }

        _controls.Monster.Shoot.performed += OnShootPressed;
        _controls.Monster.Enable();
    }

    void OnEnable()
    {
        if (_controls != null) _controls.Monster.Enable();

        _aimBarUIManager.OnAimSequenceFinished += HandleAimFinished;
        _isAiming = true;
    }

    void OnDisable()
    {
        if (_controls != null) _controls.Monster.Disable();

        _aimBarUIManager.OnAimSequenceFinished -= HandleAimFinished;
    }

    public void UpdateWindDirection(Vector3 windDirection)
    {
        _currentWindDirection = windDirection;
    }

    void Update()
    {
        if (_isAiming)
        {
            _horizontalValue = _aimBarUIManager.GetNormalized(AimBarUI.AXIS.Horizontal);
            _verticalValue = _aimBarUIManager.GetNormalized(AimBarUI.AXIS.Vertical);

            _trajectoryRenderer.RenderTrajectory(_shootTransform.position, CalculateLaunchVelocity());
        }
        else
        {
            _trajectoryRenderer.ClearTrajectory();
        }
    }

    private void OnShootPressed(InputAction.CallbackContext ctx)
    {
        if (!_isAiming) return;
        _aimBarUIManager.ProgressState();
    }

    private void HandleAimFinished(float horizontal, float vertical)
    {
        _isAiming = false;
        _horizontalValue = horizontal;
        _verticalValue = vertical;

        _animator.SetTrigger("Shoot");
        StartCoroutine(DelayShootSequence());
    }

    private IEnumerator DelayShootSequence()
    {
        yield return new WaitForSeconds(_afterShootDelay);

        Shoot();

        MudEvents.RaiseMudMonsterShot();
    }

    private Vector3 CalculateLaunchVelocity()
    {
        float widthNormalized = _horizontalValue * 2f - 1f;
        float force = Mathf.Lerp(_minForce, _maxForce, _verticalValue);

        float cameraYaw = _cameraController.GetYaw();
        Vector3 cameraForward = Quaternion.Euler(0f, cameraYaw, 0f) * Vector3.forward;

        Vector3 horizontalDir = Quaternion.AngleAxis(widthNormalized * 90f, Vector3.up) * cameraForward;
        Vector3 rightAxis = new Vector3(horizontalDir.z, 0f, -horizontalDir.x);
        Vector3 launchDir = Quaternion.AngleAxis(-_fixedAngle, rightAxis) * horizontalDir;

        return launchDir * force;
    }

    private void Shoot()
    {
        Vector3 velocity = CalculateLaunchVelocity();

        GameObject projectile = Instantiate(
            _mudProjectile,
            _shootTransform.position,
            Quaternion.LookRotation(velocity.normalized));

        if (projectile.TryGetComponent<MudProjectile>(out var mudProjectile))
        {
            mudProjectile.InitializeWindForce(_currentWindDirection);
        }

        projectile.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);
    }
}