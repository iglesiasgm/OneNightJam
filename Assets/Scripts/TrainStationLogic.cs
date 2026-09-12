using System.Collections;
using UnityEngine;

public class TrainStationLogic : MonoBehaviour
{
    [Header("Train")]
    [SerializeField] private TrainSplineFollower follower;

    [Header("Stations")]
    [SerializeField] private StationManager stationManager;

    [Header("Passengers")]
    [SerializeField] private GameClock gameClock;

    [SerializeField]
    private Transform passengerBoardingTarget;

    [Header("Station detection")]
    [SerializeField]
    private float trainTolerance = 1f;

    [Header("Braking")]
    [SerializeField]
    private float brakingDeceleration = 5f;

    private int currentStationIndex = 0;

    private bool processingStation = false;

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
        if (processingStation)
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
            traveled >= nextStation.startDistance
            &&
            traveled <= nextStation.endDistance;

        if (
            isWithinPlatform &&
            follower.GetCurrentSpeed() <= 0.1f
        )
        {
            StartCoroutine(
                OnStationReachedSuccessfully(
                    nextStation
                )
            );
        }
        else if (
            traveled >
            nextStation.endDistance +
            trainTolerance
        )
        {
            OnStationMissed(
                nextStation
            );
        }
    }

    private void OnStationMissed(
        StationManager.Station station
    )
    {
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

        currentStationIndex++;

        PrepareCurrentTargetStation();
    }

    private IEnumerator OnStationReachedSuccessfully(
        StationManager.Station station
    )
    {
        processingStation = true;

        Debug.Log(
            $"Parada correcta en " +
            $"{station.transform.name}"
        );

        StationPassengerSpawner spawner =
            GetSpawnerForStation(
                station
            );

        if (spawner != null)
        {
            if (
                passengerBoardingTarget != null
            )
            {
                yield return StartCoroutine(
                    spawner.BoardAllPassengers(
                        passengerBoardingTarget
                    )
                );
            }
            else
            {
                Debug.LogWarning(
                    "No se asignó " +
                    "PassengerBoardingTarget."
                );

                spawner
                    .ClearPassengersImmediate();
            }
        }

        currentStationIndex++;

        PrepareCurrentTargetStation();

        processingStation = false;
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
}