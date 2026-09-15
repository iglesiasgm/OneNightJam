using UnityEngine;

public class LatePassengerStationContext : MonoBehaviour
{
    [Header("Passenger path")]
    [SerializeField] private Transform runnerStartPoint;
    [SerializeField] private Transform waitingPoint;

    [Header("Station")]
    [SerializeField] private Collider stationZone;
    [SerializeField] private StationPassengerSpawner passengerSpawner;

    public Transform RunnerStartPoint =>
        runnerStartPoint;

    public Transform WaitingPoint =>
        waitingPoint;

    public Collider StationZone =>
        stationZone;

    public StationPassengerSpawner PassengerSpawner =>
        passengerSpawner;
}