using UnityEngine;
using System;
using UnityEngine.Audio;

public class MudProjectile : MonoBehaviour
{
    public static Action OnMudPuddleLand;

    [Header("References")]
    [SerializeField] private GameObject[] _mudPuddleVariants;

    [Header("Layers")]
    [SerializeField] private LayerMask _groundLayer;

    [Header("Wind Settings")]
    [SerializeField] private float _maxWindForce;

    [Header("Spawn Settings")]
    [SerializeField] private float _upOffset;

    private Rigidbody _rb;
    private Vector3 _windDirection;


    private void Awake()
    {
       _rb = GetComponent<Rigidbody>();
    }

    private void OnCollisionEnter(Collision other)
    {
        if ((_groundLayer.value & (1 << other.gameObject.layer)) == 0) return;

        SpawnMudPuddle(other);
        OnLand();
    }

    private void Update()
    {
        CheckLand();
    }

    private void CheckLand()
    {
        float minY = -1f;
        if (gameObject.transform.position.y <= minY)
        {
            OnLand();
        }
    }

    private void OnLand()
    {
        OnMudPuddleLand?.Invoke();
        Destroy(gameObject);
    }

    private void FixedUpdate()
    {
        ApplyWindForce();
        RotateTowardsVelocity();
    }

    private void ApplyWindForce()
    {
        if (_windDirection.sqrMagnitude > 0f)
        {
            Vector3 windForce = _windDirection * _maxWindForce;
            _rb?.AddForce(windForce, ForceMode.Force);
        }
    }

    private void RotateTowardsVelocity()
    {
        if (_rb.linearVelocity.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(_rb.linearVelocity);

            float smoothingSpeed = 10f;
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, smoothingSpeed * Time.fixedDeltaTime);
        }
    }

    private void SpawnMudPuddle(Collision collision)
    {
        int randomIndex = UnityEngine.Random.Range(0, _mudPuddleVariants.Length);
        GameObject selectedPuddle = _mudPuddleVariants[randomIndex];

        Vector3 spawnPosition = collision.contacts[0].point;
        spawnPosition += Vector3.up * _upOffset;

        float randomYRotation = UnityEngine.Random.Range(0f, 360f);
        Quaternion rotation = Quaternion.Euler(0f, randomYRotation, 0f);

        Instantiate(selectedPuddle, spawnPosition, rotation);
    }

    public void InitializeWindForce(Vector3 windDirection)
    {
        if (windDirection.sqrMagnitude > 0f)
        {
            _windDirection = windDirection;
        }
    }
}
