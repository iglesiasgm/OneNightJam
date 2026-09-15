using UnityEngine;

public class AnimalCrossingEvent : RandomRouteEvent
{
    [Header("References")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private GameObject animalVisual;
    [SerializeField] private Collider animalCollider;

    [Header("Activation")]
    [Tooltip("Distancia a la que debe estar el tren para activar el evento.")]
    [SerializeField] private float activationDistance = 35f;

    [Header("Crossing")]
    [SerializeField] private float minCrossingSpeed = 2f;
    [SerializeField] private float maxCrossingSpeed = 3.5f;

    [Tooltip("Tiempo aleatorio que espera el animal antes de empezar a cruzar.")]
    [SerializeField] private float minStartDelay = 0f;
    [SerializeField] private float maxStartDelay = 0.8f;

    [SerializeField] private float destinationTolerance = 0.1f;

    [Header("Cleanup")]
    [SerializeField] private float successDestroyDelay = 1f;
    [SerializeField] private float failureDestroyDelay = 0.3f;

   

    private Vector3 crossingStartPosition;
    private Vector3 crossingEndPosition;
    private Vector3 activationPosition;

    private float crossingSpeed;
    private float startDelay;
    private float delayTimer;

    private bool startedMoving = false;

    protected override void OnPrepared()
    {
        if (startPoint == null || endPoint == null)
        {
            Debug.LogError(
                $"{name}: faltan StartPoint o EndPoint."
            );

            return;
        }

        activationPosition = transform.position;

        crossingStartPosition =
            startPoint.position;

        crossingEndPosition =
            endPoint.position;

        crossingSpeed =
            Random.Range(
                minCrossingSpeed,
                maxCrossingSpeed
            );

        startDelay =
            Random.Range(
                minStartDelay,
                maxStartDelay
            );

        transform.position =
            crossingStartPosition;

        Vector3 crossingDirection =
            crossingEndPosition -
            crossingStartPosition;

        if (crossingDirection.sqrMagnitude > 0.001f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    crossingDirection.normalized,
                    Vector3.up
                );
        }

        if (animalVisual != null)
        {
            animalVisual.SetActive(false);
        }

        if (animalCollider != null)
        {
            animalCollider.enabled = false;
        }

        
    }

    private void Update()
    {
        if (
            State ==
            RandomEventState.Prepared
        )
        {
            CheckForActivation();
            return;
        }

        if (
            State !=
            RandomEventState.Active
        )
        {
            return;
        }

        UpdateCrossing();
    }

    private void CheckForActivation()
    {
        if (trainReference == null)
            return;

        float distanceToTrain =
            Vector3.Distance(
                trainReference.position,
                activationPosition
            );

        if (
            distanceToTrain <=
            activationDistance
        )
        {
            Debug.Log(
                $"Animal activado. Distancia al tren: " +
                $"{distanceToTrain:0.0} m"
            );

            ActivateEvent();
        }
    }

    protected override void OnActivated()
    {
        Debug.Log(
            $"Evento activado: {definition.DisplayName}"
        );

        delayTimer = 0f;
        startedMoving = false;

        if (animalVisual != null)
        {
            animalVisual.SetActive(true);
        }

        if (animalCollider != null)
        {
            animalCollider.enabled = true;
        }
    }

    private void UpdateCrossing()
    {
        if (!startedMoving)
        {
            delayTimer +=
                Time.deltaTime;

            if (delayTimer < startDelay)
            {
                return;
            }

            startedMoving = true;
        }

        transform.position =
            Vector3.MoveTowards(
                transform.position,
                crossingEndPosition,
                crossingSpeed *
                Time.deltaTime
            );

        float remainingDistance =
            Vector3.Distance(
                transform.position,
                crossingEndPosition
            );

        if (
            remainingDistance <=
            destinationTolerance
        )
        {
            CompleteEvent();
        }
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (
            State !=
            RandomEventState.Active
        )
        {
            return;
        }

        if (
            !other.transform.root.CompareTag(
                "Train"
            )
        )
        {
            return;
        }

        Debug.Log(
            "El tren atropelló al animal."
        );

        FailEvent();
    }

    protected override void OnSucceeded()
    {
        ReleaseSpawnPoint();

        if (animalCollider != null)
        {
            animalCollider.enabled = false;
        }

        Destroy(
            gameObject,
            successDestroyDelay
        );
    }

    protected override void OnFailed()
    {
        ReleaseSpawnPoint();

        if (animalCollider != null)
        {
            animalCollider.enabled = false;
        }

        Destroy(
            gameObject,
            failureDestroyDelay
        );
    }

    private void ReleaseSpawnPoint()
    {
        if (spawnPoint != null)
        {
            spawnPoint.SetOccupied(false);
        }
    }
}