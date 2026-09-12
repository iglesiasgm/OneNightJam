using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using UnityEngine.Events;

public class TrainDerailmentSystem : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SplineContainer splineContainer;
    [SerializeField] private TrainSplineFollower follower;

    [Header("Curvas")]
    [Tooltip("Aceleración lateral máxima tolerable antes de descarrilar (m/s²). Valores típicos de referencia: un tren real ronda 0.5-1.5 m/s² de confort, pero para gameplay podés subirlo bastante más.")]
    [SerializeField] private float maxLateralAcceleration = 8f;
    [Tooltip("Distancia usada para muestrear el cambio de tangente y estimar curvatura. Valores chicos son más precisos pero más sensibles a ruido del spline.")]
    [SerializeField] private float curvatureSampleDistance = 2f;

    [Header("Pendientes")]
    [Tooltip("Ángulo de pendiente (en grados) a partir del cual empieza a penalizarse la velocidad segura.")]
    [SerializeField] private float slopeAngleThreshold = 15f;
    [Tooltip("Cuánto se reduce la velocidad seguridad por cada grado por encima del umbral.")]
    [SerializeField] private float slopeSpeedPenaltyPerDegree = 0.05f; // 5% menos por grado

    [Header("Tolerancia de descarrilamiento")]
    [Tooltip("Cuánto tiempo (segundos) puede el tren exceder la velocidad segura antes de descarrilar. Evita fallos por picos de un solo frame.")]
    [SerializeField] private float graceTime = 0.4f;
    [Tooltip("Margen de advertencia: a partir de qué % de la velocidad segura se considera 'zona de riesgo' (para UI/feedback), antes de llegar al límite real.")]
    [Range(0f, 1f)]
    [SerializeField] private float warningThresholdPercent = 0.85f;

    public UnityEvent OnEnterDangerZone;   // para UI: parpadeo, sonido de alerta, etc.
    public UnityEvent OnExitDangerZone;
    public UnityEvent OnDerail;            // game over

    private float overSpeedTimer = 0f;
    private bool isInDangerZone = false;
    private bool hasDerailed = false;

    private void Update()
    {
        if (hasDerailed) return;

        float t = follower.GetNormalizedT();
        float currentSpeed = follower.GetCurrentSpeed();

        float curvature = EstimateCurvature(t);
        float slopeAngle = EstimateSlopeAngle(t);

        float safeSpeedForCurve = CalculateSafeSpeedForCurve(curvature);
        float safeSpeedForSlope = CalculateSafeSpeedForSlope(slopeAngle);

        // La velocidad segura real es la más restrictiva de las dos
        float safeSpeed = Mathf.Min(safeSpeedForCurve, safeSpeedForSlope);

        HandleDangerState(currentSpeed, safeSpeed);
    }

    private void HandleDangerState(float currentSpeed, float safeSpeed)
    {
        bool isOverSafeSpeed = currentSpeed > safeSpeed;
        bool isInWarningRange = currentSpeed > safeSpeed * warningThresholdPercent;

        // Feedback de advertencia (no descarrila, solo avisa)
        if (isInWarningRange && !isOverSafeSpeed)
        {
            if (!isInDangerZone)
            {
                isInDangerZone = true;
                OnEnterDangerZone?.Invoke();
            }
        }
        else if (!isOverSafeSpeed)
        {
            if (isInDangerZone)
            {
                isInDangerZone = false;
                OnExitDangerZone?.Invoke();
            }
        }

        // Lógica de descarrilamiento real
        if (isOverSafeSpeed)
        {
            overSpeedTimer += Time.deltaTime;

            if (overSpeedTimer >= graceTime)
            {
                Derail();
            }
        }
        else
        {
            overSpeedTimer = 0f;
        }
    }

    private void Derail()
    {
        float trainSpeed = follower.GetCurrentSpeed();
        Transform trainTransform = follower.transform;
        GetComponentInChildren<TrainDerailAnimation>().BeginAnimation(trainTransform, trainSpeed);
        
        hasDerailed = true;
        follower.SetSpeed(0f);
        OnDerail?.Invoke();
        
        Debug.Log("El tren descarriló.");
    }

    // --- Cálculo de curvatura ---
    // Estima qué tanto "gira" el spline por metro recorrido, muestreando la tangente
    // en dos puntos cercanos y midiendo el cambio de ángulo (proyectado en el plano horizontal).
    private float EstimateCurvature(float t)
    {
        float splineLength = follower.GetSplineLength();
        float dt = curvatureSampleDistance / splineLength;

        float tA = Mathf.Clamp01(t - dt * 0.5f);
        float tB = Mathf.Clamp01(t + dt * 0.5f);

        Vector3 tangentA = ((Vector3)(float3)splineContainer.EvaluateTangent(tA)).normalized;
        Vector3 tangentB = ((Vector3)(float3)splineContainer.EvaluateTangent(tB)).normalized;

        // Ignoramos el componente vertical para no mezclar curvatura horizontal con pendiente
        tangentA.y = 0f;
        tangentB.y = 0f;
        tangentA.Normalize();
        tangentB.Normalize();

        float angleDelta = Vector3.Angle(tangentA, tangentB) * Mathf.Deg2Rad;
        float arcLength = Mathf.Max(curvatureSampleDistance, 0.001f);

        return angleDelta / arcLength; // curvatura en rad/metro
    }

    // --- Cálculo de pendiente ---
    private float EstimateSlopeAngle(float t)
    {
        Vector3 tangent = ((Vector3)(float3)splineContainer.EvaluateTangent(t)).normalized;
        // El ángulo respecto al plano horizontal
        return Mathf.Asin(Mathf.Clamp(tangent.y, -1f, 1f)) * Mathf.Rad2Deg;
    }

    // --- Velocidad segura según curvatura ---
    // v_safe = sqrt(a_max / curvatura). Si curvatura es ~0 (tramo recto), no hay límite práctico.
    private float CalculateSafeSpeedForCurve(float curvature)
    {
        if (curvature < 0.0001f) return Mathf.Infinity;
        return Mathf.Sqrt(maxLateralAcceleration / curvature);
    }

    // --- Velocidad segura según pendiente ---
    private float CalculateSafeSpeedForSlope(float slopeAngle)
    {
        float absAngle = Mathf.Abs(slopeAngle);
        if (absAngle <= slopeAngleThreshold) return Mathf.Infinity;

        float excessDegrees = absAngle - slopeAngleThreshold;
        float penaltyFactor = Mathf.Clamp01(1f - excessDegrees * slopeSpeedPenaltyPerDegree);

        // Sin un tope de referencia, devolvemos un valor relativo alto multiplicado por el factor.
        // Ajustá "100f" a un valor de referencia acorde a tu maxSpeed real del tren.
        return 100f * penaltyFactor;
    }
}