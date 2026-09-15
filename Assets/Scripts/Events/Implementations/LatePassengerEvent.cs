using System.Collections;
using UnityEngine;

public class LatePassengerEvent : RandomRouteEvent
{
    [Header("Passenger")]
    [SerializeField] private Transform passengerVisual;

    [Header("Activation")]
    [SerializeField] private float activationDistance = 40f;

    [Header("Running")]
    [SerializeField] private float minRunningSpeed = 3f;
    [SerializeField] private float maxRunningSpeed = 5f;

    [SerializeField] private float minStartDelay = 0.2f;
    [SerializeField] private float maxStartDelay = 1.5f;

    [Header("Boarding")]
    [SerializeField] private float boardingDuration = 0.5f;
    [SerializeField] private float stoppedSpeedThreshold = 0.1f;

    [Header("Cleanup")]
    [SerializeField] private float successDestroyDelay = 0.5f;
    [SerializeField] private float failureDestroyDelay = 1f;

    private LatePassengerStationContext stationContext;

    private Vector3 runnerStartPosition;
    private Vector3 waitingPosition;

    private float runningSpeed;
    private float startDelay;
    private float delayTimer;

    private bool startedRunning = false;
    private bool reachedPlatform = false;
    private bool trainHasEnteredStation = false;
    private bool boardingInProgress = false;

    protected override void OnPrepared()
    {
        stationContext =
            spawnPoint.GetComponent<
                LatePassengerStationContext
            >();

        if (stationContext == null)
        {
            Debug.LogError(
                $"{name}: el EventSpawnPoint no tiene " +
                $"LatePassengerStationContext."
            );

            return;
        }

        if (
            stationContext.RunnerStartPoint == null ||
            stationContext.WaitingPoint == null ||
            stationContext.StationZone == null
        )
        {
            Debug.LogError(
                $"{name}: faltan referencias del contexto de estación."
            );

            return;
        }

        if (passengerVisual == null)
        {
            Debug.LogError(
                $"{name}: falta Passenger Visual."
            );

            return;
        }

        runnerStartPosition =
            stationContext.RunnerStartPoint.position;

        waitingPosition =
            stationContext.WaitingPoint.position;

        passengerVisual.position =
            runnerStartPosition;

        FaceTowards(
            waitingPosition
        );

        runningSpeed =
            Random.Range(
                minRunningSpeed,
                maxRunningSpeed
            );

        startDelay =
            Random.Range(
                minStartDelay,
                maxStartDelay
            );

        delayTimer = 0f;

        passengerVisual.gameObject.SetActive(
            false
        );
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

        UpdateStationState();

        if (
            State !=
            RandomEventState.Active
        )
        {
            return;
        }

        UpdatePassenger();
    }

    private void CheckForActivation()
    {
        if (trainReference == null)
            return;

        float distance =
            Vector3.Distance(
                trainReference.position,
                spawnPoint.transform.position
            );

        if (
            distance <=
            activationDistance
        )
        {
            ActivateEvent();
        }
    }

    protected override void OnActivated()
    {
        Debug.Log(
            $"Evento activado: " +
            $"{definition.DisplayName}"
        );

        passengerVisual.gameObject.SetActive(
            true
        );

        passengerVisual.position =
            runnerStartPosition;

        delayTimer = 0f;
        startedRunning = false;
        reachedPlatform = false;
        trainHasEnteredStation = false;
        boardingInProgress = false;
    }

    private void UpdateStationState()
    {
        bool trainInsideStation =
            IsTrainInsideStation();

        if (trainInsideStation)
        {
            trainHasEnteredStation = true;
        }

        if (
            trainHasEnteredStation &&
            !trainInsideStation
        )
        {
            Debug.Log(
                "El tren se fue sin esperar " +
                "al pasajero retrasado."
            );

            FailEvent();
        }
    }

    private void UpdatePassenger()
    {
        if (!startedRunning)
        {
            delayTimer +=
                Time.deltaTime;

            if (delayTimer < startDelay)
                return;

            startedRunning = true;

            FaceTowards(
                waitingPosition
            );
        }

        if (!reachedPlatform)
        {
            passengerVisual.position =
                Vector3.MoveTowards(
                    passengerVisual.position,
                    waitingPosition,
                    runningSpeed *
                    Time.deltaTime
                );

            float distanceToPlatform =
                Vector3.Distance(
                    passengerVisual.position,
                    waitingPosition
                );

            if (
                distanceToPlatform <=
                0.1f
            )
            {
                passengerVisual.position =
                    waitingPosition;

                reachedPlatform = true;

                Debug.Log(
                    "El pasajero retrasado llegó a la estación."
                );
            }

            return;
        }

        TryToBoard();
    }

    private void TryToBoard()
    {
        if (boardingInProgress)
            return;

        if (!IsTrainInsideStation())
            return;

        if (!IsTrainStopped())
            return;

        StationPassengerSpawner spawner =
            stationContext.PassengerSpawner;

        if (
            spawner != null &&
            spawner.PassengerCount > 0
        )
        {
            return;
        }

        Transform boardingTarget =
            eventManager.PassengerBoardingTarget;

        if (boardingTarget == null)
        {
            Debug.LogWarning(
                "PassengerBoardingTarget no asignado " +
                "en RandomEventManager."
            );

            return;
        }

        StartCoroutine(
            BoardLatePassenger(
                boardingTarget
            )
        );
    }

    private IEnumerator BoardLatePassenger(
        Transform boardingTarget
    )
    {
        boardingInProgress = true;

        Vector3 startPosition =
            passengerVisual.position;

        Vector3 startScale =
            passengerVisual.localScale;

        float elapsed = 0f;

        float duration =
            Mathf.Max(
                0.01f,
                boardingDuration
            );

        while (elapsed < duration)
        {
            if (
                !IsTrainInsideStation() ||
                !IsTrainStopped()
            )
            {
                passengerVisual.position =
                    startPosition;

                passengerVisual.localScale =
                    startScale;

                boardingInProgress = false;

                yield break;
            }

            float t =
                elapsed / duration;

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            passengerVisual.position =
                Vector3.Lerp(
                    startPosition,
                    boardingTarget.position,
                    smoothT
                );

            passengerVisual.localScale =
                Vector3.Lerp(
                    startScale,
                    startScale * 0.1f,
                    smoothT
                );

            elapsed +=
                Time.deltaTime;

            yield return null;
        }

        passengerVisual.position =
            boardingTarget.position;

        passengerVisual.localScale =
            startScale * 0.1f;

        passengerVisual.gameObject.SetActive(
            false
        );

        boardingInProgress = false;

        CompleteEvent();
    }

    private bool IsTrainStopped()
    {
        if (
            eventManager.TrainController ==
            null
        )
        {
            return false;
        }

        return
            eventManager
                .TrainController
                .GetCurrentSpeed()
            <=
            stoppedSpeedThreshold;
    }

    private bool IsTrainInsideStation()
    {
        if (
            trainReference == null ||
            stationContext == null ||
            stationContext.StationZone == null
        )
        {
            return false;
        }

        Vector3 closestPoint =
            stationContext
                .StationZone
                .ClosestPoint(
                    trainReference.position
                );

        float distance =
            Vector3.Distance(
                closestPoint,
                trainReference.position
            );

        return distance <= 0.01f;
    }

    private void FaceTowards(
        Vector3 target
    )
    {
        Vector3 direction =
            target -
            passengerVisual.position;

        direction.y = 0f;

        if (
            direction.sqrMagnitude >
            0.001f
        )
        {
            passengerVisual.rotation =
                Quaternion.LookRotation(
                    direction.normalized,
                    Vector3.up
                );
        }
    }

    protected override void OnSucceeded()
    {
        ReleaseSpawnPoint();

        Destroy(
            gameObject,
            successDestroyDelay
        );
    }

    protected override void OnFailed()
    {
        ReleaseSpawnPoint();

        Destroy(
            gameObject,
            failureDestroyDelay
        );
    }

    private void ReleaseSpawnPoint()
    {
        if (spawnPoint != null)
        {
            spawnPoint.SetOccupied(
                false
            );
        }
    }
}