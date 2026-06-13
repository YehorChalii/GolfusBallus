using UnityEngine;

public class TrajectoryRenderer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private LineRenderer _lineRenderer;
    [SerializeField] private int _trajectoryPoints;
    [SerializeField] private float _trajectoryTimeStep;

    public void RenderTrajectory(Vector3 origin, Vector3 launchVelocity)
    {
        if (_lineRenderer == null) return;

        _lineRenderer.positionCount = _trajectoryPoints;
        float time = 0f;

        for (int i = 0; i < _trajectoryPoints; i++)
        {
            float x = launchVelocity.x * time + (Physics.gravity.x / 2f) * time * time;
            float y = launchVelocity.y * time + (Physics.gravity.y / 2f) * time * time;
            float z = launchVelocity.z * time + (Physics.gravity.z / 2f) * time * time;

            _lineRenderer.SetPosition(i, origin + new Vector3(x, y, z));
            time += _trajectoryTimeStep;
        }
    }

    public void ClearTrajectory()
    {
        if (_lineRenderer != null)
        {
            _lineRenderer.positionCount = 0;
        }
    }
}