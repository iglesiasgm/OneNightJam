using UnityEngine;

public class TrainStationLogic : MonoBehaviour
{
    [SerializeField] private TrainSplineFollower follower;
    [SerializeField] private StationManager stationManager;
    [SerializeField] private float trainTolerance = 1f;
    [SerializeField] private float brakingDeceleration = 5f; // unidades/seg²

    private int currentStationIndex = 0;

    public float GetBrakingDistanceNeeded(float currentSpeed)
    {
        // Física básica: d = v² / (2a)
        return (currentSpeed * currentSpeed) / (2f * brakingDeceleration);
    }

    private void Update()
    {
        if (currentStationIndex >= stationManager.Stations.Count) return;

        var nextStation = stationManager.Stations[currentStationIndex];
        float traveled = follower.GetDistanceTraveled();
        bool isWithinPlatform = traveled >= nextStation.startDistance && traveled <= nextStation.endDistance;


        if (isWithinPlatform && follower.GetCurrentSpeed() <= 0.1f)
        {
            OnStationReachedSuccessfully(nextStation);
        }
        else if (traveled > nextStation.endDistance + trainTolerance)
        {
            OnStationMissed(nextStation);
        }
    }

    private void OnStationMissed(StationManager.Station station)
    {
        Debug.Log($"Te pasaste de la estación {station.transform.name}");
        currentStationIndex++; // o penalizar, game over, etc.
    }

    private void OnStationReachedSuccessfully(StationManager.Station station)
    {
        Debug.Log($"Parada correcta en {station.transform.name}");
        currentStationIndex++;
    }
}