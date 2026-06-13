using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class MudMonsterController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject _mudProjectile;
    [SerializeField] private Transform _shootTransform;
    [SerializeField] private Transform _cameraTransform;
    [SerializeField] private AimBarUIManager _aimBarUIManager;
    [SerializeField] private TrajectoryRenderer _trajectoryRenderer;

    [Header("Shoot Settings")]
    [SerializeField] private float _fixedAngle;
    [SerializeField] private float _minForce;
    [SerializeField] private float _maxForce;
    [SerializeField] private float _afterShootDelay = 0.5f;

    [Header("Animations")]
    [SerializeField] private Animator _animator;

    private InputSystem_Actions _controls;
    private bool _isAiming = false;

    private float _horizontalValue = 0.5f;
    private float _verticalValue = 0.5f;

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

        AimBarUIManager.OnAimSequenceFinished += HandleAimFinished;
        _isAiming = true;
    }

    void OnDisable()
    {
        if (_controls != null) _controls.Monster.Disable();

        AimBarUIManager.OnAimSequenceFinished -= HandleAimFinished;
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
        GameManager.Instance.OnMonsterShoot();
    }

    private Vector3 CalculateLaunchVelocity()
    {
        float widthNormalized = _horizontalValue * 2f - 1f;
        float force = Mathf.Lerp(_minForce, _maxForce, _verticalValue);

        Vector3 cameraForward = _cameraTransform.forward;
        cameraForward.y = 0f;
        if (cameraForward.sqrMagnitude < 0.001f) cameraForward = Vector3.forward;
        cameraForward.Normalize();

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
            mudProjectile.InitializeWindForce(GameManager.Instance.CurrentWindDirection);
        }

        projectile.GetComponent<Rigidbody>().AddForce(velocity, ForceMode.Impulse);

        SoundManager.PlaySound(SoundType.SFX_MudShot, 0.7f);
    }
}