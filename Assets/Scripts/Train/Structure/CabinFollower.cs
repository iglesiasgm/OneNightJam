using UnityEngine;

public class CabinFollower : SplineFollower
{
    [SerializeField] private float currentSpeed = 0f;
    
    private float distanceTraveled = 0f;

    private void Update()
    {
        distanceTraveled += currentSpeed * Time.deltaTime;
        distanceTraveled = ClampOrRepeat(distanceTraveled);
        ApplyTransformAtDistance(distanceTraveled);
    }

    public void SetSpeed(float speed) => currentSpeed = speed;
    public float GetCurrentSpeed() => currentSpeed;
    public float GetDistanceTraveled() => distanceTraveled;
    public float GetNormalizedT() => splineLength > 0f ? distanceTraveled / splineLength : 0f;
}