using UnityEngine;

public class AimArrowRenderer : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LineRenderer line;

    [Header("Arrow Settings")]
    [SerializeField] private float maxLength;

    [Header("Color Feedback")]
    [SerializeField] private Color lowPowerColor;
    [SerializeField] private Color highPowerColor;

    [Header("Pulse")]
    [SerializeField, Range(0f, 0.2f)] private float pulseAmount;
    [SerializeField, Range(1f, 10f)] private float pulseSpeed;

    private float _pulse;

    void Awake()
    {
        line.positionCount = 2;
        Hide();
    }

    public void UpdateArrow(Vector3 origin, Vector3 launchDirection, float power)
    {
        _pulse += Time.deltaTime * pulseSpeed;
        float sineWave = Mathf.Sin(_pulse);

        float pulsedLength = power * maxLength * (1f + sineWave * pulseAmount);
        Vector3 tip = origin + launchDirection * pulsedLength;

        float minLength = pulsedLength * 0.1f;

        line.SetPosition(0, origin);
        line.SetPosition(1, tip - launchDirection * minLength);

        Color color = Color.Lerp(lowPowerColor, highPowerColor, power);

        line.startColor = color;
        line.endColor = color;
    }

    public void Hide()
    {
        line.enabled = false;
        _pulse = 0f;
    }

    public void Show()
    {
        line.enabled = true;
    }
}