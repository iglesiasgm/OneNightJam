using UnityEngine;
using UnityEngine.Splines;

public abstract class SplineFollower : MonoBehaviour
{
    [SerializeField] protected SplineContainer splineContainer;
    [SerializeField] protected bool alignToTangent = true;
    [SerializeField] protected bool loop = false;

    protected float splineLength;

    protected virtual void Awake()
    {
        if (splineContainer != null)
        {
            splineLength = splineContainer.CalculateLength();
        }
    }

    public float GetSplineLength() => splineLength;

    protected float ClampOrRepeat(float distance)
    {
        return loop ? Mathf.Repeat(distance, splineLength) : Mathf.Clamp(distance, 0f, splineLength);
    }

    protected void ApplyTransformAtDistance(float distance)
    {
        if (splineLength <= 0f) return;

        float t = distance / splineLength;
        Vector3 localPosition = splineContainer.EvaluatePosition(t);
        transform.position = splineContainer.transform.TransformPoint(localPosition);

        if (alignToTangent)
        {
            Vector3 tangent = splineContainer.EvaluateTangent(t);
            Vector3 worldTangent = splineContainer.transform.TransformDirection(tangent).normalized;

            if (worldTangent != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(worldTangent, Vector3.up);
            }
        }
    }
}