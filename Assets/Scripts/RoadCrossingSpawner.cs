using System;
using UnityEngine;

public class RoadCrossingSpawner : MonoBehaviour
{
    [Serializable]
    public class RoadLane
    {
        public Transform spawnPoint;
        public Transform stopPoint;
        public Transform endPoint;
    }

    [Header("References")]
    [SerializeField] private GameClock gameClock;

    [SerializeField]
    private RoadCrossingController crossingController;

    [SerializeField]
    private GameManager gameManager;

    [Header("Vehicles")]
    [SerializeField]
    private GameObject[] vehiclePrefabs;

    [Header("Lanes")]
    [SerializeField]
    private RoadLane[] lanes;

    [Header("Weekday traffic")]
    [SerializeField]
    private float weekdayPeakMinInterval = 1.3f;

    [SerializeField]
    private float weekdayPeakMaxInterval = 2.5f;

    [SerializeField]
    private float weekdayOffPeakMinInterval = 3.5f;

    [SerializeField]
    private float weekdayOffPeakMaxInterval = 6f;

    [Header("Weekend traffic")]
    [SerializeField]
    private float weekendPeakMinInterval = 2f;

    [SerializeField]
    private float weekendPeakMaxInterval = 4f;

    [SerializeField]
    private float weekendOffPeakMinInterval = 5f;

    [SerializeField]
    private float weekendOffPeakMaxInterval = 8f;

    private float spawnTimer;
    private float nextSpawnTime;

    private void Start()
    {
        ScheduleNextSpawn();
    }

    private void Update()
    {
        spawnTimer +=
            Time.deltaTime;

        if (
            spawnTimer <
            nextSpawnTime
        )
        {
            return;
        }

        SpawnVehicle();

        spawnTimer = 0f;

        ScheduleNextSpawn();
    }

    private void SpawnVehicle()
    {
        if (
            vehiclePrefabs == null ||
            vehiclePrefabs.Length == 0
        )
        {
            return;
        }

        if (
            lanes == null ||
            lanes.Length == 0
        )
        {
            return;
        }

        RoadLane lane =
            lanes[
                UnityEngine.Random.Range(
                    0,
                    lanes.Length
                )
            ];

        if (
            lane.spawnPoint == null ||
            lane.stopPoint == null ||
            lane.endPoint == null
        )
        {
            return;
        }

        GameObject prefab =
            vehiclePrefabs[
                UnityEngine.Random.Range(
                    0,
                    vehiclePrefabs.Length
                )
            ];

        GameObject vehicle =
            Instantiate(
                prefab,
                lane.spawnPoint.position,
                lane.spawnPoint.rotation
            );

        RoadVehicle roadVehicle =
            vehicle.GetComponent<
                RoadVehicle
            >();

        if (roadVehicle == null)
        {
            Debug.LogError(
                $"{prefab.name} no tiene RoadVehicle."
            );

            Destroy(vehicle);

            return;
        }

        roadVehicle.Initialize(
            crossingController,
            lane.spawnPoint,
            lane.stopPoint,
            lane.endPoint,
            gameManager
        );
    }

    private void ScheduleNextSpawn()
    {
        bool weekend =
            IsWeekend();

        bool peak =
            IsPeakHour(
                weekend
            );

        float minInterval;
        float maxInterval;

        if (!weekend && peak)
        {
            minInterval =
                weekdayPeakMinInterval;

            maxInterval =
                weekdayPeakMaxInterval;
        }
        else if (!weekend)
        {
            minInterval =
                weekdayOffPeakMinInterval;

            maxInterval =
                weekdayOffPeakMaxInterval;
        }
        else if (peak)
        {
            minInterval =
                weekendPeakMinInterval;

            maxInterval =
                weekendPeakMaxInterval;
        }
        else
        {
            minInterval =
                weekendOffPeakMinInterval;

            maxInterval =
                weekendOffPeakMaxInterval;
        }

        nextSpawnTime =
            UnityEngine.Random.Range(
                minInterval,
                maxInterval
            );
    }

    private bool IsWeekend()
    {
        if (gameClock == null)
            return false;

        return
            gameClock.CurrentDay ==
            DayOfWeek.Saturday
            ||
            gameClock.CurrentDay ==
            DayOfWeek.Sunday;
    }

    private bool IsPeakHour(
        bool weekend
    )
    {
        if (gameClock == null)
            return false;

        float hour =
            gameClock.CurrentHour;

        if (weekend)
        {
            bool middayPeak =
                hour >= 12f &&
                hour < 14f;

            bool nightPeak =
                hour >= 21f &&
                hour < 23f;

            return
                middayPeak ||
                nightPeak;
        }

        bool morningPeak =
            hour >= 6f &&
            hour < 8f;

        bool middayPeakWeekday =
            hour >= 12f &&
            hour < 14f;

        bool eveningPeak =
            hour >= 18f &&
            hour < 20.5f;

        return
            morningPeak ||
            middayPeakWeekday ||
            eveningPeak;
    }
}