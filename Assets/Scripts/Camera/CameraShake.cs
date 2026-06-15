using UnityEngine;

public class CameraShake : MonoBehaviour
{
    [Header("Shake Settings")]
    [SerializeField] private float _maxShakeFrequency;
    [SerializeField] private float _maxShakeMagnitude;

    private float _shakeTimer;

    public void ProcessShake(float normalizedIntensity)
    {
        if (normalizedIntensity > 0.01f)
        {
            _shakeTimer += Time.deltaTime * (_maxShakeFrequency * normalizedIntensity);

            float noiseX = Mathf.PerlinNoise(_shakeTimer, 0.0f) * 2f - 1f;
            float noiseY = Mathf.PerlinNoise(0.0f, _shakeTimer) * 2f - 1f;

            float powerCurve = normalizedIntensity * normalizedIntensity;
            float currentMagnitude = _maxShakeMagnitude * powerCurve;

            transform.localPosition = (Vector3.right * noiseX + Vector3.up * noiseY) * currentMagnitude;
        }
        else
        {
            _shakeTimer = 0f;
            transform.localPosition = Vector3.zero;
        }
    }
}