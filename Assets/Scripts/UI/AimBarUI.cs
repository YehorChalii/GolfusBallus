using UnityEngine;

public class AimBarUI : MonoBehaviour
{
    public enum AXIS { Horizontal, Vertical }
    [SerializeField] private AXIS _axis;
    [SerializeField] private RectTransform _rangeRectangle;
    [SerializeField] private RectTransform _indicator;

    [Header("Sin Wave Settings")]
    [SerializeField] private bool _invertSinWave;
    [SerializeField, Range(0f, 1f)] private float _slowPoint;
    [SerializeField, Range(0f, 3f)] private float _minSpeed;
    [SerializeField, Range(0f, 3f)] private float _maxSpeed;

    public float Value01 { get; private set; }
    public AXIS Axis => _axis;

    private bool _movingForward = true;
    private bool _isActive = false;

    void Awake()
    {
        Value01 = 0f;
    }

    public void SetActive(bool active)
    {
        _isActive = active;
        gameObject.SetActive(active);
    }

    public void ResetBar()
    {
        Value01 = 0f;
        _movingForward = true;
        UpdateIndicatorPosition();
    }

    void Update()
    {
        if (!_isActive) return;

        float speed = CalculateSinSpeed();

        if (_movingForward)
        {
            Value01 += speed * Time.deltaTime;
            if (Value01 >= 1f) _movingForward = false;
        }
        else
        {
            Value01 -= speed * Time.deltaTime;
            if (Value01 <= 0f) _movingForward = true;
        }

        Value01 = Mathf.Clamp01(Value01);
        UpdateIndicatorPosition();
    }

    private float CalculateSinSpeed()
    {
        float centerBias = Mathf.Cos((Value01 - _slowPoint) * Mathf.PI);
        centerBias = Mathf.Clamp01(centerBias);

        if (_invertSinWave)
        {
            return Mathf.Lerp(_minSpeed, _maxSpeed, centerBias);
        }
        else
        {
            return Mathf.Lerp(_maxSpeed, _minSpeed, centerBias);
        }
    }


    private void UpdateIndicatorPosition()
    {
        float range = _axis == AXIS.Horizontal
            ? _rangeRectangle.rect.width
            : _rangeRectangle.rect.height;

        float pos = Value01 * range;

        if (_axis == AXIS.Horizontal)
            _indicator.anchoredPosition = new Vector2(pos, _indicator.anchoredPosition.y);
        else
            _indicator.anchoredPosition = new Vector2(_indicator.anchoredPosition.x, pos);
    }
}
