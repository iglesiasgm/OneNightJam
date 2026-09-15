using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class RandomEventManager :
    MonoBehaviour
{

    [Header("Train")]
    [SerializeField] private Transform trainReference;

    [Header("Events")]
    [SerializeField]
    private List<RandomEventDefinition>
        availableEvents;

    [Header("Spawn Points")]
    [SerializeField]
    private List<EventSpawnPoint>
        spawnPoints;

    [Header("Level configuration")]
    [Min(0)]
    [SerializeField]
    private int minimumEvents = 2;

    [Min(0)]
    [SerializeField]
    private int maximumEvents = 4;

    [Min(0f)]
    [SerializeField]
    private float minimumDistanceBetweenEvents =
        30f;

    public event Action<
        RandomEventDefinition
    > OnEventSucceeded;

    public event Action<
        RandomEventDefinition
    > OnEventFailed;

    private readonly List<EventSpawnPoint>
        usedSpawnPoints =
            new List<EventSpawnPoint>();

    private void Start()
    {
        Debug.Log("RANDOM EVENT MANAGER INICIADO");
        GenerateEvents();
    }

    private void GenerateEvents()
    {
        if (
            availableEvents == null ||
            availableEvents.Count == 0
        )
        {
            Debug.LogWarning(
                "No hay eventos configurados."
            );

            return;
        }

        if (
            spawnPoints == null ||
            spawnPoints.Count == 0
        )
        {
            Debug.LogWarning(
                "No hay EventSpawnPoints."
            );

            return;
        }

        int targetEventCount =
            Random.Range(
                minimumEvents,
                maximumEvents + 1
            );

        int generatedEvents = 0;

        int safetyCounter = 0;

        while (
            generatedEvents <
            targetEventCount &&
            safetyCounter < 100
        )
        {
            safetyCounter++;

            RandomEventDefinition definition =
                GetRandomEventDefinition();

            if (definition == null)
                continue;

            EventSpawnPoint point =
                FindAvailableSpawnPoint(
                    definition
                );

            if (point == null)
                continue;

            SpawnEvent(
                definition,
                point
            );

            generatedEvents++;
        }

        Debug.Log(
            $"Eventos generados: " +
            $"{generatedEvents}"
        );
    }

    private RandomEventDefinition
        GetRandomEventDefinition()
    {
        float totalWeight = 0f;

        foreach (
            RandomEventDefinition definition
            in availableEvents
        )
        {
            if (definition == null)
                continue;

            totalWeight +=
                Mathf.Max(
                    0f,
                    definition.Weight
                );
        }

        if (totalWeight <= 0f)
            return null;

        float randomValue =
            Random.Range(
                0f,
                totalWeight
            );

        float accumulatedWeight = 0f;

        foreach (
            RandomEventDefinition definition
            in availableEvents
        )
        {
            if (definition == null)
                continue;

            accumulatedWeight +=
                Mathf.Max(
                    0f,
                    definition.Weight
                );

            if (
                randomValue <=
                accumulatedWeight
            )
            {
                return definition;
            }
        }

        return null;
    }

    private EventSpawnPoint
        FindAvailableSpawnPoint(
            RandomEventDefinition definition
        )
    {
        List<EventSpawnPoint>
            candidates =
                new List<EventSpawnPoint>();

        foreach (
            EventSpawnPoint point
            in spawnPoints
        )
        {
            if (point == null)
                continue;

            if (point.IsOccupied)
                continue;

            if (
                point.SpawnType !=
                definition.SpawnType
            )
            {
                continue;
            }

            if (
                !HasEnoughDistance(
                    point
                )
            )
            {
                continue;
            }

            candidates.Add(
                point
            );
        }

        if (candidates.Count == 0)
            return null;

        return candidates[
            Random.Range(
                0,
                candidates.Count
            )
        ];
    }

    private bool HasEnoughDistance(
        EventSpawnPoint candidate
    )
    {
        foreach (
            EventSpawnPoint usedPoint
            in usedSpawnPoints
        )
        {
            float distance =
                Vector3.Distance(
                    candidate.transform.position,
                    usedPoint.transform.position
                );

            if (
                distance <
                minimumDistanceBetweenEvents
            )
            {
                return false;
            }
        }

        return true;
    }

    private void SpawnEvent(
        RandomEventDefinition definition,
        EventSpawnPoint point
    )
    {
        GameObject eventObject =
            Instantiate(
                definition.EventPrefab,
                point.SpawnTransform.position,
                point.SpawnTransform.rotation
            );

        RandomRouteEvent routeEvent =
            eventObject.GetComponent<
                RandomRouteEvent
            >();

        if (routeEvent == null)
        {
            Debug.LogError(
                $"{definition.EventPrefab.name} " +
                $"no tiene RandomRouteEvent."
            );

            Destroy(eventObject);

            return;
        }

        point.SetOccupied(
            true
        );

        usedSpawnPoints.Add(
            point
        );

        routeEvent.Initialize(
            this,
            definition,
            point,
            trainReference
        );
    }

    public void EventSucceeded(
        RandomRouteEvent routeEvent
    )
    {
        RandomEventDefinition definition =
            routeEvent.Definition;

        Debug.Log(
            $"{definition.DisplayName}: " +
            $"+${definition.SuccessReward}"
        );

        OnEventSucceeded?.Invoke(
            definition
        );
    }

    public void EventFailed(
        RandomRouteEvent routeEvent
    )
    {
        RandomEventDefinition definition =
            routeEvent.Definition;

        Debug.Log(
            $"{definition.DisplayName}: " +
            $"-${definition.FailurePenalty}"
        );

        OnEventFailed?.Invoke(
            definition
        );
    }
}