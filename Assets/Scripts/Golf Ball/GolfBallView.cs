using UnityEngine;
using UnityEngine.UI;

public class GolfBallView : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GolfBallController _golfBallController;
    [SerializeField] private AimArrowRenderer _aimArrow;

    [Header("Hat Settings")]
    [SerializeField] private Transform _hatTransform;
    [SerializeField] private float _hatYOffset;
    [SerializeField] private float _hatMoveSmoothTime;
    [SerializeField] private float _hatRotateSpeed;

    [Header("UI")]
    [SerializeField] private CanvasGroup _mudEffect;
    [SerializeField] private float _mudFadeInSpeed;
    [SerializeField] private float _mudFadeOutSpeed;

    [Header("Speed Effect UI")]
    [SerializeField] private Image _speedEffectImage;

    [Space]
    [SerializeField] private float _maxTransitionValue;
    [SerializeField] private string _transitionShaderProperty = "_TransitionValue";
    [SerializeField] private string _timeMultiplierProperty = "_ViewportTimeMultiplier";

    [Header("Particles")]
    [SerializeField] private ParticleSystem _launchParticles;

    private Vector3 _hatMoveVelocity;
    private Quaternion _targetHatRotation;

    private bool _aimArrowShown;

    private float _targetMudAlpha = 0f;
    private float _currentFadeSpeed;
    private Material _speedEffectMaterial;

    void Awake()
    {
        _targetHatRotation = _hatTransform.rotation;

        _mudEffect.alpha = 0f;
        _mudEffect.gameObject.SetActive(true);
        _currentFadeSpeed = _mudFadeInSpeed;

        _speedEffectMaterial = _speedEffectImage.material;
        _speedEffectImage.gameObject.SetActive(false);
    }

    void OnEnable()
    {
        _golfBallController.OnAimUpdated += HandleAimUpdated;
        _golfBallController.OnAimCanceled += HandleAimCanceled;
        _golfBallController.OnBallLaunched += HandleBallLaunched;
        _golfBallController.OnMudEntered += HandleMudEntered;
        _golfBallController.OnMudExited += HandleMudExited;

        SetShaderTimeMultiplier(1);
    }

    void OnDisable()
    {
        _golfBallController.OnAimUpdated -= HandleAimUpdated;
        _golfBallController.OnAimCanceled -= HandleAimCanceled;
        _golfBallController.OnBallLaunched -= HandleBallLaunched;
        _golfBallController.OnMudEntered -= HandleMudEntered;
        _golfBallController.OnMudExited -= HandleMudExited;

        SetShaderTimeMultiplier(0);
    }

    void LateUpdate()
    {
        UpdateHatTransform();
        UpdateMudFade();
    }

    private void HandleAimUpdated(Vector3 launchDirection, float normalizedPower)
    {
        if (!_aimArrowShown)
        {
            _aimArrow.Show();
            SoundManager.PlaySound(SoundType.SFX_GolfBallCharge, 0.1f);
        }

        _aimArrowShown = true;
        _aimArrow.UpdateArrow(transform.position, launchDirection, normalizedPower);

        if (launchDirection != Vector3.zero)
        {
            _targetHatRotation = Quaternion.LookRotation(launchDirection);
        }

        if (normalizedPower > 0.05f)
        {
            _speedEffectImage.gameObject.SetActive(true);

            float currentTransition = Mathf.Lerp(0f, _maxTransitionValue, normalizedPower);

            _speedEffectMaterial.SetFloat(_transitionShaderProperty, currentTransition);
        }
        else
        {
            _speedEffectImage.gameObject.SetActive(false);
        }
    }

    private void HandleAimCanceled()
    {
        _aimArrow.Hide();
        _aimArrowShown = false;

        _speedEffectImage.gameObject.SetActive(false);
    }

    private void HandleBallLaunched(Vector3 launchDirection, float force)
    {
        HandleAimCanceled();
        SoundManager.PlaySound(SoundType.SFX_GolfBallLaunch, 0.7f);

        if (_launchParticles != null)
        {
            _launchParticles.Play();
        }
    }

    private void HandleMudEntered()
    {
        SoundManager.PlaySound(SoundType.SFX_MudSplash, 0.3f);

        _targetMudAlpha = 1f;
        _currentFadeSpeed = _mudFadeInSpeed;
    }

    private void HandleMudExited()
    {
        _targetMudAlpha = 0f;
        _currentFadeSpeed = _mudFadeOutSpeed;
    }

    private void UpdateHatTransform()
    {
        Vector3 targetPosition = transform.position + Vector3.up * _hatYOffset;
        _hatTransform.position = Vector3.SmoothDamp(_hatTransform.position, targetPosition, ref _hatMoveVelocity, _hatMoveSmoothTime);
        _hatTransform.rotation = Quaternion.Slerp(_hatTransform.rotation, _targetHatRotation, Time.deltaTime * _hatRotateSpeed);
    }

    private void UpdateMudFade()
    {
        if (_mudEffect == null) return;

        if (!Mathf.Approximately(_mudEffect.alpha, _targetMudAlpha))
        {
            _mudEffect.alpha = Mathf.MoveTowards(
                _mudEffect.alpha,
                _targetMudAlpha,
                _currentFadeSpeed * Time.deltaTime
            );
        }
    }

    private void SetShaderTimeMultiplier(int value)
    {
        if (_speedEffectMaterial == null && _speedEffectImage != null)
        {
            _speedEffectMaterial = _speedEffectImage.material;
        }

        if (_speedEffectMaterial != null)
        {
            _speedEffectMaterial.SetInt(_timeMultiplierProperty, value);
        }
    }
}