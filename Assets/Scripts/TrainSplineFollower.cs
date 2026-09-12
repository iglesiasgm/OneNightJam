using UnityEngine;
using UnityEngine.Splines;

public class TrainSplineFollower : MonoBehaviour
{
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private bool alignToTangent = true;
    [SerializeField] private bool loop = false;

    private float distanceTraveled = 0f;
    private float splineLength;

    private void Start()
    {
        splineLength = splineContainer.CalculateLength();
    }

    private void Update()
    {
        // Acá ssimplemente actualizá distanceTraveled con la velocidad actual,
        // que podés modificar desde afuera en cualquier momento (con tu script de aceleración)
        distanceTraveled += currentSpeed * Time.deltaTime;

        if (loop)
        {
            distanceTraveled = Mathf.Repeat(distanceTraveled, splineLength);
        }
        else
        {
            distanceTraveled = Mathf.Clamp(distanceTraveled, 0f, splineLength);
        }

        float t = distanceTraveled / splineLength; // normalizado 0-1

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

    // Método público para que tu script de aceleración lo controle
    public void SetSpeed(float speed)
    {
        currentSpeed = speed;
    }

    public float GetCurrentSpeed() => currentSpeed;
    
    public float GetDistanceTraveled() => distanceTraveled;

    public float GetProgress01() => distanceTraveled / splineLength;
}