using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    [Header("Rain Setup Settings")]
    [SerializeField] private GameObject _mudProjectilePrefab;
    [SerializeField] private int _rainProjectileCount;
    [SerializeField] private float _rainRadius;
    [SerializeField] private float _rainSpawnHeight;
    [SerializeField] private float _mudPuddleIntersectionRadius;

    private const int _maxAttemptsPerProjectile = 50;

    [Header("Wind Particles")]
    [SerializeField] private ParticleSystem _windForwardParticles;
    [SerializeField] private ParticleSystem _windBackParticles;
    [SerializeField] private ParticleSystem _windLeftParticles;
    [SerializeField] private ParticleSystem _windRightParticles;

    public void SpawnRainProjectiles()
    {
        List<Vector3> spawnedPositions = new List<Vector3>();

        for (int i = 0; i < _rainProjectileCount; i++)
        {
            Vector3 spawnPos = GetValidRainSpawnPosition(spawnedPositions);
            spawnedPositions.Add(spawnPos);
            Instantiate(_mudProjectilePrefab, spawnPos, _mudProjectilePrefab.transform.rotation);
        }

        RoundEvents.RaiseRainSetupComplete();
    }

    private Vector3 GetValidRainSpawnPosition(List<Vector3> existingPositions)
    {
        Vector3 spawnPos = Vector3.zero;
        bool isValidPosition = false;
        int attempts = 0;

        while (!isValidPosition && attempts < _maxAttemptsPerProjectile)
        {
            attempts++;
            Vector2 randomPoint = UnityEngine.Random.insideUnitCircle * _rainRadius;
            spawnPos = new Vector3(randomPoint.x, _rainSpawnHeight, randomPoint.y);

            isValidPosition = CheckRainSpawnPositionValidity(spawnPos, existingPositions);
        }

        return spawnPos;
    }

    private bool CheckRainSpawnPositionValidity(Vector3 newPos, List<Vector3> existingPositions)
    {
        Vector3 flatNew = new Vector3(newPos.x, 0f, newPos.z);
        float doubleRadius = _mudPuddleIntersectionRadius * 2f;

        foreach (Vector3 existingPos in existingPositions)
        {
            Vector3 flatExisting = new Vector3(existingPos.x, 0f, existingPos.z);
            if (Vector3.Distance(flatNew, flatExisting) < doubleRadius)
            {
                return false;
            }
        }
        return true;
    }

    public void ApplyWindParticlesToggle(Vector3 direction)
    {
        _windForwardParticles.Stop();
        _windBackParticles.Stop();
        _windLeftParticles.Stop();
        _windRightParticles.Stop();

        if (direction.sqrMagnitude < 0.001f) return;

        if (direction == Vector3.forward) _windForwardParticles.Play();
        else if (direction == Vector3.back) _windBackParticles.Play();
        else if (direction == Vector3.left) _windLeftParticles.Play();
        else if (direction == Vector3.right) _windRightParticles.Play();
    }
}