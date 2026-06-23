using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    [Header("Orbit Settings")]
    [SerializeField] private float _baseDistance;
    [SerializeField] private float _height;
    [SerializeField] private float _orbitSpeed;
    [SerializeField] private float _positionSmoothing;
    private float _yaw;
    private float _pitch = 20f;
    private Vector3 _posVelocity;

    [Header("Constrains")]
    [SerializeField] private float _minPitch;
    [SerializeField] private float _maxPitch;

    private Transform _target;

    private float _distanceMultiplier = 1f;

    private float _targetYawOverride;
    private float _yawOverrideSpeed;
    private bool _isOverridingYaw;

    public float CurrentYaw => _yaw;

    public void Setup(Transform target)
    {
        _target = target;
        _yaw = transform.eulerAngles.y;
        float wrappedPitch = transform.eulerAngles.x;
        _pitch = (wrappedPitch > 180) ? wrappedPitch - 360 : wrappedPitch;
    }

    public void ProcessOrbitInput(Vector2 lookInput)
    {
        if (_isOverridingYaw) _isOverridingYaw = false;

        _yaw += lookInput.x * _orbitSpeed * Time.deltaTime;
        _pitch += lookInput.y * _orbitSpeed * Time.deltaTime;
        _pitch = Mathf.Clamp(_pitch, _minPitch, _maxPitch);
    }

    public void SetDistanceMultiplier(float multiplier)
    {
        _distanceMultiplier = multiplier;
    }

    public void OverrideYawAngle(float targetYaw, float speed)
    {
        _targetYawOverride = targetYaw;
        _yawOverrideSpeed = speed;
        _isOverridingYaw = true;
    }

    public void ExecuteUpdate()
    {
        if (_target == null) return;

        if (_isOverridingYaw)
        {
            _yaw = Mathf.LerpAngle(_yaw, _targetYawOverride, _yawOverrideSpeed * Time.deltaTime);
            if (Mathf.Abs(Mathf.DeltaAngle(_yaw, _targetYawOverride)) <= 0.05f)
            {
                _yaw = _targetYawOverride;
                _isOverridingYaw = false;
            }
        }

        float finalDistance = _baseDistance * _distanceMultiplier;
        Quaternion rotationMatrix = Quaternion.Euler(_pitch, _yaw, 0f);
        Vector3 targetPosition = _target.position + (rotationMatrix * new Vector3(0f, 0f, -finalDistance)) + (Vector3.up * _height);

        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref _posVelocity, _positionSmoothing);
        transform.LookAt(_target.position);
    }
}