using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TrainPassengerManager : MonoBehaviour
{
    [Header("Capacity")]
    [SerializeField] private int passengerCapacity = 30;

    private readonly List<int> passengerDestinations =
        new List<int>();

    public int PassengerCapacity =>
        passengerCapacity;

    public int PassengerCount =>
        passengerDestinations.Count;

    public int AvailableCapacity =>
        Mathf.Max(
            0,
            passengerCapacity -
            passengerDestinations.Count
        );

    public bool IsFull =>
        AvailableCapacity <= 0;

    public event Action OnPassengerBoarded;
    public event Action<int> OnPassengersDisembarked;

    public bool TryBoardPassenger(
        int currentStationIndex,
        int totalStations
    )
    {
        if (IsFull)
            return false;

        if (
            currentStationIndex >=
            totalStations - 1
        )
        {
            return false;
        }

        int destinationStation =
            Random.Range(
                currentStationIndex + 1,
                totalStations
            );

        passengerDestinations.Add(
            destinationStation
        );

        OnPassengerBoarded?.Invoke();

        Debug.Log(
            $"Pasajero subió. " +
            $"Destino: estación {destinationStation}. " +
            $"A bordo: {PassengerCount}/" +
            $"{PassengerCapacity}"
        );

        return true;
    }

    public int DisembarkPassengers(
        int stationIndex
    )
    {
        int passengersLeaving = 0;

        for (
            int i =
                passengerDestinations.Count - 1;
            i >= 0;
            i--
        )
        {
            if (
                passengerDestinations[i] ==
                stationIndex
            )
            {
                passengerDestinations.RemoveAt(
                    i
                );

                passengersLeaving++;
            }
        }

        if (passengersLeaving > 0)
        {
            OnPassengersDisembarked?.Invoke(
                passengersLeaving
            );

            Debug.Log(
                $"{passengersLeaving} pasajeros bajaron. " +
                $"A bordo: {PassengerCount}/" +
                $"{PassengerCapacity}"
            );
        }

        return passengersLeaving;
    }

    public void SetPassengerCapacity(
        int newCapacity
    )
    {
        passengerCapacity =
            Mathf.Max(
                1,
                newCapacity
            );
    }
}