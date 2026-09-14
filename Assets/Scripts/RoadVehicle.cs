using UnityEngine;

public class RoadVehicle : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float minCruiseSpeed = 7f;
    [SerializeField] private float maxCruiseSpeed = 11f;

    [SerializeField] private float acceleration = 3f;

    [Header("Braking")]
    [SerializeField] private float minBrakingDeceleration = 6f;
    [SerializeField] private float maxBrakingDeceleration = 10f;

    [Header("Driver reaction")]
    [SerializeField] private float minReactionTime = 0.05f;
    [SerializeField] private float maxReactionTime = 0.30f;

    [Header("Traffic spacing")]
    [SerializeField] private float minimumVehicleGap = 1.5f;

    [Tooltip("Distancia a la que empieza a reducir velocidad si hay otro vehículo adelante.")]
    [SerializeField] private float slowdownDistance = 6f;

    [Tooltip("Distancia máxima usada para detectar vehículos adelante.")]
    [SerializeField] private float vehicleDetectionDistance = 12f;

    [Tooltip("Qué tan fuerte frena para no alcanzar al vehículo de adelante.")]
    [SerializeField] private float trafficBrakingDeceleration = 10f;

    private RoadCrossingController crossingController;

    private Transform stopPoint;
    private Transform endPoint;

    private GameManager gameManager;

    private Vector3 direction;

    private float cruiseSpeed;
    private float currentSpeed;

    private float brakingDeceleration;

    private float reactionDelay;
    private float reactionTimer;

    private bool crossingWasClosed = false;
    private bool initialized = false;
    private bool crashed = false;

    public void Initialize(
        RoadCrossingController crossing,
        Transform spawnPoint,
        Transform stop,
        Transform end,
        GameManager manager
    )
    {
        crossingController = crossing;

        stopPoint = stop;
        endPoint = end;

        gameManager = manager;

        transform.position =
            spawnPoint.position;

        direction =
            (
                endPoint.position -
                spawnPoint.position
            ).normalized;

        transform.rotation =
            Quaternion.LookRotation(
                direction,
                Vector3.up
            );

        cruiseSpeed =
            Random.Range(
                minCruiseSpeed,
                maxCruiseSpeed
            );

        currentSpeed =
            cruiseSpeed;

        brakingDeceleration =
            Random.Range(
                minBrakingDeceleration,
                maxBrakingDeceleration
            );

        reactionDelay =
            Random.Range(
                minReactionTime,
                maxReactionTime
            );

        reactionTimer = 0f;

        initialized = true;
    }

    private void Update()
    {
        if (!initialized || crashed)
            return;

        bool crossingClosed =
            crossingController != null &&
            crossingController.IsClosed;

        float distanceToStopLine =
            Vector3.Dot(
                stopPoint.position -
                transform.position,
                direction
            );

        bool beforeStopLine =
            distanceToStopLine > 0f;

        if (
            crossingClosed &&
            beforeStopLine
        )
        {
            HandleClosedCrossing();
        }
        else
        {
            HandleOpenCrossing();
        }

        HandleVehicleAhead();

        MoveVehicle();

        CheckEndPoint();

        crossingWasClosed =
            crossingClosed;
    }

    private void HandleClosedCrossing()
    {
        if (!crossingWasClosed)
        {
            reactionTimer = 0f;

            reactionDelay =
                Random.Range(
                    minReactionTime,
                    maxReactionTime
                );
        }

        if (reactionTimer < reactionDelay)
        {
            reactionTimer +=
                Time.deltaTime;

            return;
        }

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                0f,
                brakingDeceleration *
                Time.deltaTime
            );
    }

    private void HandleOpenCrossing()
    {
        reactionTimer = 0f;

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                cruiseSpeed,
                acceleration *
                Time.deltaTime
            );
    }

    private void HandleVehicleAhead()
    {
        float distanceToVehicleAhead =
            GetDistanceToVehicleAhead();

        if (
            float.IsInfinity(
                distanceToVehicleAhead
            )
        )
        {
            return;
        }

        if (
            distanceToVehicleAhead >=
            slowdownDistance
        )
        {
            return;
        }

        float speedFactor =
            Mathf.InverseLerp(
                minimumVehicleGap,
                slowdownDistance,
                distanceToVehicleAhead
            );

        float targetSpeed =
            cruiseSpeed *
            speedFactor;

        currentSpeed =
            Mathf.MoveTowards(
                currentSpeed,
                targetSpeed,
                trafficBrakingDeceleration *
                Time.deltaTime
            );
    }

    private float GetDistanceToVehicleAhead()
    {
        BoxCollider ownCollider =
            GetComponent<BoxCollider>();

        float halfLength = 0f;

        if (ownCollider != null)
        {
            halfLength =
                ownCollider.size.z *
                transform.lossyScale.z *
                0.5f;
        }

        Vector3 rayOrigin =
            transform.position +
            direction *
            (halfLength + 0.05f);

        RaycastHit[] hits =
            Physics.RaycastAll(
                rayOrigin,
                direction,
                vehicleDetectionDistance,
                Physics.DefaultRaycastLayers,
                QueryTriggerInteraction.Collide
            );

        float closestDistance =
            Mathf.Infinity;

        foreach (RaycastHit hit in hits)
        {
            RoadVehicle otherVehicle =
                hit.collider
                    .GetComponentInParent<RoadVehicle>();

            if (
                otherVehicle == null ||
                otherVehicle == this
            )
            {
                continue;
            }

            if (
                hit.distance <
                closestDistance
            )
            {
                closestDistance =
                    hit.distance;
            }
        }

        return closestDistance;
    }

    private void MoveVehicle()
    {
        float moveDistance =
            currentSpeed *
            Time.deltaTime;

        float distanceToVehicleAhead =
            GetDistanceToVehicleAhead();

        if (
            !float.IsInfinity(
                distanceToVehicleAhead
            )
        )
        {
            float availableDistance =
                Mathf.Max(
                    0f,
                    distanceToVehicleAhead -
                    minimumVehicleGap
                );

            moveDistance =
                Mathf.Min(
                    moveDistance,
                    availableDistance
                );

            if (
                availableDistance <=
                0.001f
            )
            {
                currentSpeed = 0f;
            }
        }

        transform.position +=
            direction *
            moveDistance;
    }

    private void CheckEndPoint()
    {
        float distanceToEnd =
            Vector3.Dot(
                endPoint.position -
                transform.position,
                direction
            );

        if (distanceToEnd <= 0f)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (crashed)
            return;

        if (
            !other.transform.root.CompareTag(
                "Train"
            )
        )
        {
            return;
        }

        crashed = true;
        currentSpeed = 0f;

        Debug.Log(
            "Un vehículo chocó contra el tren."
        );

        if (gameManager != null)
        {
            gameManager
                .GameOverRoadCollision();
        }
    }
}