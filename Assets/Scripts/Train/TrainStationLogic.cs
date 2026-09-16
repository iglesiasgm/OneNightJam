using System.Collections;
using UnityEngine;
using System;

public class TrainStationLogic : MonoBehaviour
{
    [Header("Game")]
    [SerializeField] private GameManager gameManager;

    [Header("Train")]
    [SerializeField] private CabinFollower follower;

    [Header("Stations")]
    [SerializeField] private StationManager stationManager;

    [Header("Passengers")]
    [SerializeField] private GameClock gameClock;

    [Header("Passenger Capacity")]
    [SerializeField] private TrainPassengerManager trainPassengerManager;

    [SerializeField]
    private Transform passengerBoardingTarget;

    [Header("Station detection")]
    [SerializeField]
    private float trainTolerance = 1f;

    [Header("Braking")]
    [SerializeField]
    private float brakingDeceleration = 5f;

    private int currentStationIndex = 0;

    private bool boardingProcessActive = false;
    private bool gameOverTriggered = false;

    public event Action<int, int> OnStationPassengerExchangeCompleted;

    private void Start()
    {
        PrepareCurrentTargetStation();
    }

    public float GetBrakingDistanceNeeded(
        float currentSpeed
    )
    {
        return
            (currentSpeed * currentSpeed) /
            (2f * brakingDeceleration);
    }

    private void Update()
    {
        if (gameOverTriggered)
            return;

        if (
            currentStationIndex >=
            stationManager.Stations.Count
        )
        {
            return;
        }

        var nextStation =
            stationManager.Stations[
                currentStationIndex
            ];

        float traveled =
            follower.GetDistanceTraveled();

        bool isWithinPlatform =
            traveled >= nextStation.startDistance &&
            traveled <= nextStation.endDistance;

        if (
            traveled >
            nextStation.endDistance +
            trainTolerance
        )
        {
            OnStationMissed(
                nextStation
            );

            return;
        }

        if (
            isWithinPlatform &&
            follower.GetCurrentSpeed() <= 0.1f &&
            !boardingProcessActive
        )
        {
            StartCoroutine(
                BoardPassengersSequentially(
                    nextStation
                )
            );
        }
    }

    private void OnStationMissed(
    StationManager.Station station
)
    {
        if (gameOverTriggered)
            return;

        gameOverTriggered = true;

        Debug.Log(
            $"Te pasaste de la estación " +
            $"{station.transform.name}"
        );

        StationPassengerSpawner spawner =
            GetSpawnerForStation(
                station
            );

        if (spawner != null)
        {
            spawner.ClearPassengersImmediate();
        }

        gameManager.GameOverMissedStation();
    }

    private IEnumerator BoardPassengersSequentially(
    StationManager.Station station
)
    {
        boardingProcessActive = true;

        Debug.Log(
            $"Parada correcta en " +
            $"{station.transform.name}"
        );

        int passengersLeaving = 0;

        if (trainPassengerManager != null)
        {
            passengersLeaving =
                trainPassengerManager
                    .DisembarkPassengers(
                        currentStationIndex
                    );
        }

        int passengersBoarded = 0;

        if (passengersLeaving > 0)
        {
            Debug.Log(
                $"Bajaron {passengersLeaving} pasajeros " +
                $"en {station.transform.name}."
            );
        }

        StationPassengerSpawner spawner =
            GetSpawnerForStation(
                station
            );

        if (spawner == null)
        {
            Debug.LogWarning(
                $"{station.transform.name} " +
                $"no tiene StationPassengerSpawner."
            );

            CompleteCurrentStation();

            boardingProcessActive = false;

            yield break;
        }

        if (passengerBoardingTarget == null)
        {
            Debug.LogWarning(
                "No se asignó " +
                "PassengerBoardingTarget."
            );

            spawner.ClearPassengersImmediate();

            CompleteCurrentStation();

            boardingProcessActive = false;

            yield break;
        }

        while (
            spawner.PassengerCount > 0 &&
            !gameOverTriggered
        )
        {

            if (
    trainPassengerManager != null &&
    trainPassengerManager.IsFull
)
            {
                Debug.Log(
                    $"Tren lleno. " +
                    $"{spawner.PassengerCount} pasajeros " +
                    $"quedaron en la estación."
                );

                spawner.ClearPassengersImmediate();

                break;
            }

            if (
                !CanPassengersBoard(
                    station
                )
            )
            {
                yield return null;
                continue;
            }

            bool passengerBoarded =
                false;

            yield return StartCoroutine(
                spawner.BoardOnePassenger(
                    passengerBoardingTarget,

                    () =>
                        CanPassengersBoard(
                            station
                        ),

                    success =>
                    {
                        passengerBoarded =
                            success;
                    }
                )
            );

            if (passengerBoarded)
            {
                if (trainPassengerManager != null)
                {
                    bool registered =
                        trainPassengerManager
                            .TryBoardPassenger(
                                currentStationIndex,
                                stationManager.Stations.Count
                            );

                    if (registered)
                    {
                        passengersBoarded++;
                    }
                }
            }
            else
            {
                yield return null;
            }
        }

        if (gameOverTriggered)
        {
            boardingProcessActive =
                false;

            yield break;
        }

        Debug.Log(
            $"Todos los pasajeros subieron en " +
            $"{station.transform.name}"
        );

        OnStationPassengerExchangeCompleted?.Invoke(passengersLeaving, passengersBoarded);

        CompleteCurrentStation();

        boardingProcessActive = false;
    }

    private bool CanPassengersBoard(
    StationManager.Station station
)
    {
        float traveled =
            follower.GetDistanceTraveled();

        bool isWithinPlatform =
            traveled >= station.startDistance &&
            traveled <= station.endDistance;

        bool trainIsStopped =
            follower.GetCurrentSpeed() <= 0.1f;

        return
            isWithinPlatform &&
            trainIsStopped;
    }

    private void CompleteCurrentStation()
    {
        currentStationIndex++;
        PrepareCurrentTargetStation();
    }

    private void PrepareCurrentTargetStation()
    {
        if (
            currentStationIndex >=
            stationManager.Stations.Count
        )
        {
            Debug.Log(
                "No quedan más estaciones."
            );

            return;
        }

        StationManager.Station station =
            stationManager.Stations[
                currentStationIndex
            ];

        Debug.Log(
            $"Próxima estación: " +
            $"{station.transform.name}"
        );

        StationPassengerSpawner spawner =
            GetSpawnerForStation(
                station
            );

        if (spawner == null)
        {
            Debug.LogWarning(
                $"{station.transform.name} " +
                $"no tiene " +
                $"StationPassengerSpawner."
            );

            return;
        }

        spawner.PreparePassengers(
            gameClock
        );
    }

    private StationPassengerSpawner
        GetSpawnerForStation(
            StationManager.Station station
        )
    {
        return station.transform
            .GetComponentInChildren<
                StationPassengerSpawner
            >();
    }

    public string GetNextStationName()
    {
        if (
            stationManager == null ||
            currentStationIndex < 0 ||
            currentStationIndex >= stationManager.Stations.Count
        )
        {
            return "Fin de línea";
        }

        return stationManager
            .Stations[currentStationIndex]
            .transform.name;
    }

    public float GetDistanceToNextStation()
    {
        if (
            stationManager == null ||
            follower == null ||
            currentStationIndex < 0 ||
            currentStationIndex >= stationManager.Stations.Count
        )
        {
            return 0f;
        }

        StationManager.Station nextStation =
            stationManager.Stations[
                currentStationIndex
            ];

        float traveled =
            follower.GetDistanceTraveled();

        float remainingDistance =
            nextStation.startDistance -
            traveled;

        return Mathf.Max(
            0f,
            remainingDistance
        );
    }
}