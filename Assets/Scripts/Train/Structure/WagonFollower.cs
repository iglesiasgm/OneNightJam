using UnityEngine;

public class WagonFollower : SplineFollower
{
    [SerializeField] private float couplingDistance = 5f;
    
    public float CouplingDistance => couplingDistance;
    
    // Llamado por TrainConsist cada frame con la distancia ya calculada
    public void UpdatePosition(float targetDistance)
    {
        float clamped = ClampOrRepeat(targetDistance);
        ApplyTransformAtDistance(clamped);
    }
}