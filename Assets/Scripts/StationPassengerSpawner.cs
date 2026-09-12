using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Random = UnityEngine.Random;

public class StationPassengerSpawner : MonoBehaviour
{
    [Header("Passenger prefab")]
    [SerializeField] private GameObject passengerPrefab;

    [Header("Spawn zones")]
    [SerializeField] private List<BoxCollider> spawnZones;

    [Header("Weekday population")]
    [SerializeField] private int weekdayOffPeakMin = 4;
    [SerializeField] private int weekdayOffPeakMax = 10;

    [SerializeField] private int weekdayPeakMin = 18;
    [SerializeField] private int weekdayPeakMax = 35;

    [Header("Weekend population")]
    [SerializeField] private int weekendOffPeakMin = 3;
    [SerializeField] private int weekendOffPeakMax = 8;

    [SerializeField] private int weekendPeakMin = 12;
    [SerializeField] private int weekendPeakMax = 24;

    [Header("Passenger position")]
    [SerializeField] private float passengerHalfHeight = 0.9f;

    [Header("Boarding animation")]
    [SerializeField] private float boardingDuration = 0.8f;
    [SerializeField] private float maxBoardingDelay = 0.25f;

    private readonly List<GameObject> spawnedPassengers =
        new List<GameObject>();

    private Transform passengerContainer;

    public int PassengerCount => spawnedPassengers.Count;

    public void PreparePassengers(GameClock gameClock)
    {
        if (gameClock == null)
        {
            Debug.LogError($"{name}: falta GameClock.");
            return;
        }

        if (passengerPrefab == null)
        {
            Debug.LogError($"{name}: falta Passenger Prefab.");
            return;
        }

        if (spawnZones == null || spawnZones.Count == 0)
        {
            Debug.LogError($"{name}: no hay Spawn Zones configuradas.");
            return;
        }

        EnsurePassengerContainer();

        ClearPassengersImmediate();

        int amount =
            CalculatePassengerAmount(gameClock);

        for (int i = 0; i < amount; i++)
        {
            SpawnPassenger();
        }

        Debug.Log(
            $"{name} | " +
            $"{gameClock.CurrentDay} " +
            $"{gameClock.CurrentTimeText} | " +
            $"Pasajeros generados: {amount}"
        );
    }

    public IEnumerator BoardAllPassengers(
        Transform boardingTarget
    )
    {
        if (boardingTarget == null)
        {
            Debug.LogError(
                $"{name}: Boarding Target es null."
            );

            yield break;
        }

        if (spawnedPassengers.Count == 0)
        {
            yield break;
        }

        List<GameObject> passengers =
            new List<GameObject>(
                spawnedPassengers
            );

        spawnedPassengers.Clear();

        int passengersRemaining =
            passengers.Count;

        foreach (GameObject passenger in passengers)
        {
            if (passenger == null)
            {
                passengersRemaining--;
                continue;
            }

            float randomDelay =
                Random.Range(
                    0f,
                    maxBoardingDelay
                );

            StartCoroutine(
                MovePassengerToTrain(
                    passenger,
                    boardingTarget,
                    randomDelay,
                    () =>
                    {
                        passengersRemaining--;
                    }
                )
            );
        }

        while (passengersRemaining > 0)
        {
            yield return null;
        }
    }

    public void ClearPassengersImmediate()
    {
        for (
            int i = spawnedPassengers.Count - 1;
            i >= 0;
            i--
        )
        {
            if (spawnedPassengers[i] != null)
            {
                Destroy(
                    spawnedPassengers[i]
                );
            }
        }

        spawnedPassengers.Clear();
    }

    private IEnumerator MovePassengerToTrain(
        GameObject passenger,
        Transform boardingTarget,
        float delay,
        Action onFinished
    )
    {
        if (delay > 0f)
        {
            yield return new WaitForSeconds(
                delay
            );
        }

        if (passenger == null)
        {
            onFinished?.Invoke();
            yield break;
        }

        Vector3 startPosition =
            passenger.transform.position;

        Vector3 startScale =
            passenger.transform.localScale;

        float elapsed = 0f;

        float duration =
            Mathf.Max(
                0.01f,
                boardingDuration
            );

        while (
            elapsed < duration &&
            passenger != null
        )
        {
            float t =
                elapsed / duration;

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            passenger.transform.position =
                Vector3.Lerp(
                    startPosition,
                    boardingTarget.position,
                    smoothT
                );

            passenger.transform.localScale =
                Vector3.Lerp(
                    startScale,
                    startScale * 0.1f,
                    smoothT
                );

            elapsed += Time.deltaTime;

            yield return null;
        }

        if (passenger != null)
        {
            Destroy(passenger);
        }

        onFinished?.Invoke();
    }

    private int CalculatePassengerAmount(
        GameClock gameClock
    )
    {
        bool weekend =
            gameClock.CurrentDay ==
            DayOfWeek.Saturday
            ||
            gameClock.CurrentDay ==
            DayOfWeek.Sunday;

        bool peak =
            IsPeakHour(
                gameClock.CurrentHour,
                weekend
            );

        if (!weekend && peak)
        {
            return Random.Range(
                weekdayPeakMin,
                weekdayPeakMax + 1
            );
        }

        if (!weekend)
        {
            return Random.Range(
                weekdayOffPeakMin,
                weekdayOffPeakMax + 1
            );
        }

        if (peak)
        {
            return Random.Range(
                weekendPeakMin,
                weekendPeakMax + 1
            );
        }

        return Random.Range(
            weekendOffPeakMin,
            weekendOffPeakMax + 1
        );
    }

    private bool IsPeakHour(
    float hour,
    bool weekend
)
    {
        if (weekend)
        {
            bool weekendMiddayPeak =
                hour >= 12f &&
                hour < 14f;

            bool weekendNightPeak =
                hour >= 21f &&
                hour < 23f;

            return
                weekendMiddayPeak ||
                weekendNightPeak;
        }

        bool weekdayMorningPeak =
            hour >= 6f &&
            hour < 8f;

        bool weekdayMiddayPeak =
            hour >= 12f &&
            hour < 14f;

        bool weekdayEveningPeak =
            hour >= 18f &&
            hour < 20.5f;

        return
            weekdayMorningPeak ||
            weekdayMiddayPeak ||
            weekdayEveningPeak;
    }

    private void SpawnPassenger()
    {
        BoxCollider zone =
            GetRandomSpawnZone();

        if (zone == null)
            return;

        Vector3 position =
            GetRandomPointInsideZone(zone);

        Quaternion rotation =
            Quaternion.Euler(
                0f,
                Random.Range(0f, 360f),
                0f
            );

        GameObject passenger =
            Instantiate(
                passengerPrefab,
                position,
                rotation,
                passengerContainer
            );

        spawnedPassengers.Add(
            passenger
        );
    }

    private BoxCollider GetRandomSpawnZone()
    {
        List<BoxCollider> validZones =
            new List<BoxCollider>();

        foreach (
            BoxCollider zone
            in spawnZones
        )
        {
            if (zone != null)
            {
                validZones.Add(zone);
            }
        }

        if (validZones.Count == 0)
            return null;

        return validZones[
            Random.Range(
                0,
                validZones.Count
            )
        ];
    }

    private Vector3 GetRandomPointInsideZone(
        BoxCollider zone
    )
    {
        float halfX =
            zone.size.x * 0.5f;

        float halfZ =
            zone.size.z * 0.5f;

        float randomX =
            Random.Range(
                -halfX,
                halfX
            );

        float randomZ =
            Random.Range(
                -halfZ,
                halfZ
            );

        float topY =
            zone.center.y +
            zone.size.y * 0.5f;

        Vector3 localPoint =
            new Vector3(
                zone.center.x + randomX,
                topY,
                zone.center.z + randomZ
            );

        Vector3 worldPoint =
            zone.transform.TransformPoint(
                localPoint
            );

        worldPoint +=
            zone.transform.up *
            passengerHalfHeight;

        return worldPoint;
    }

    private void EnsurePassengerContainer()
    {
        if (passengerContainer != null)
            return;

        Transform existing =
            transform.Find(
                "GeneratedPassengers"
            );

        if (existing != null)
        {
            passengerContainer =
                existing;

            return;
        }

        GameObject container =
            new GameObject(
                "GeneratedPassengers"
            );

        container.transform.SetParent(
            transform
        );

        passengerContainer =
            container.transform;
    }
}