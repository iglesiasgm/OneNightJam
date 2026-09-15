using UnityEngine;

public class CrossingEntityEvent : RandomRouteEvent
{
    [Header("Actor")]
    [SerializeField] private Transform actor;
    [SerializeField] private Collider actorCollider;
    [SerializeField] private CrossingEntityHitbox hitbox;

    [Header("Crossing Points")]
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;

    [Header("Activation")]
    [SerializeField] private float activationDistance = 35f;

    [Header("Movement")]
    [SerializeField] private float minCrossingSpeed = 2f;
    [SerializeField] private float maxCrossingSpeed = 3.5f;

    [SerializeField] private float minStartDelay = 0f;
    [SerializeField] private float maxStartDelay = 0.8f;

    [SerializeField] private float destinationTolerance = 0.1f;

    [Header("Cleanup")]
    [SerializeField] private float successDestroyDelay = 1f;
    [SerializeField] private float failureDestroyDelay = 0.3f;

    private Vector3 startPosition;
    private Vector3 endPosition;
    private Vector3 activationPosition;

    private float crossingSpeed;
    private float startDelay;
    private float delayTimer;

    private bool startedMoving = false;

    protected override void OnPrepared()
    {
        if (
            actor == null ||
            startPoint == null ||
            endPoint == null
        )
        {
            Debug.LogError(
                $"{name}: faltan referencias del CrossingEntityEvent."
            );

            return;
        }

        activationPosition =
            transform.position;

        startPosition =
            startPoint.position;

        endPosition =
            endPoint.position;

        actor.position =
            startPosition;

        Vector3 direction =
            endPosition -
            startPosition;

        if (direction.sqrMagnitude > 0.001f)
        {
            actor.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );
        }

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

        delayTimer = 0f;
        startedMoving = false;

        if (hitbox != null)
        {
            hitbox.Initialize(this);
        }

        if (actorCollider != null)
        {
            actorCollider.enabled = false;
        }

        actor.gameObject.SetActive(false);
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

        float distance =
            Vector3.Distance(
                trainReference.position,
                activationPosition
            );

        if (distance <= activationDistance)
        {
            ActivateEvent();
        }
    }

    protected override void OnActivated()
    {
        Debug.Log(
            $"Evento activado: {definition.DisplayName}"
        );

        actor.gameObject.SetActive(true);

        if (actorCollider != null)
        {
            actorCollider.enabled = true;
        }

        delayTimer = 0f;
        startedMoving = false;
    }

    private void UpdateCrossing()
    {
        if (!startedMoving)
        {
            delayTimer +=
                Time.deltaTime;

            if (delayTimer < startDelay)
                return;

            startedMoving = true;
        }

        actor.position =
            Vector3.MoveTowards(
                actor.position,
                endPosition,
                crossingSpeed *
                Time.deltaTime
            );

        float remainingDistance =
            Vector3.Distance(
                actor.position,
                endPosition
            );

        if (
            remainingDistance <=
            destinationTolerance
        )
        {
            CompleteEvent();
        }
    }

    public void OnHitByTrain()
    {
        if (
            State !=
            RandomEventState.Active
        )
        {
            return;
        }

        Debug.Log(
            $"El tren impactó: {definition.DisplayName}"
        );

        FailEvent();
    }

    protected override void OnSucceeded()
    {
        ReleaseSpawnPoint();

        if (actorCollider != null)
        {
            actorCollider.enabled = false;
        }

        Destroy(
            gameObject,
            successDestroyDelay
        );
    }

    protected override void OnFailed()
    {
        ReleaseSpawnPoint();

        if (actorCollider != null)
        {
            actorCollider.enabled = false;
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