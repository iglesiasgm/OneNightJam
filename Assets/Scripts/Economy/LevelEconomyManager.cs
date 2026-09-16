using UnityEngine;

public class LevelEconomyManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private TrainPassengerManager passengerManager;

    [SerializeField]
    private RandomEventManager randomEventManager;

    [Header("Ticket")]
    [SerializeField]
    private int ticketPrice = 2;

    public int SuccessfulEvents
    {
        get;
        private set;
    }

    public int FailedEvents
    {
        get;
        private set;
    }

    public int PassengersTransported
    {
        get;
        private set;
    }

    public int PassengerRevenue
    {
        get;
        private set;
    }

    public int EventRewards
    {
        get;
        private set;
    }

    public int EventPenalties
    {
        get;
        private set;
    }

    public int TotalRevenue =>
        PassengerRevenue +
        EventRewards -
        EventPenalties;

    private void OnEnable()
    {
        if (passengerManager != null)
        {
            passengerManager.OnPassengerBoarded +=
                RegisterPassengerBoarded;
        }

        if (randomEventManager != null)
        {
            randomEventManager.OnEventSucceeded +=
                RegisterEventSuccess;

            randomEventManager.OnEventFailed +=
                RegisterEventFailure;
        }
    }

    private void OnDisable()
    {
        if (passengerManager != null)
        {
            passengerManager.OnPassengerBoarded -=
                RegisterPassengerBoarded;
        }

        if (randomEventManager != null)
        {
            randomEventManager.OnEventSucceeded -=
                RegisterEventSuccess;

            randomEventManager.OnEventFailed -=
                RegisterEventFailure;
        }
    }

    private void RegisterPassengerBoarded()
    {
        PassengersTransported++;

        PassengerRevenue +=
            ticketPrice;

        Debug.Log(
            $"Boleto vendido: +${ticketPrice}. " +
            $"Recaudación actual: ${TotalRevenue}"
        );
    }

    private void RegisterEventSuccess(
        RandomEventDefinition definition
    )
    {
        if (definition == null)
            return;

        SuccessfulEvents++;

        EventRewards +=
            definition.SuccessReward;

        Debug.Log(
            $"Bonus de evento: " +
            $"+${definition.SuccessReward}. " +
            $"Recaudación actual: ${TotalRevenue}"
        );
    }

    private void RegisterEventFailure(
        RandomEventDefinition definition
    )
    {
        if (definition == null)
            return;

        FailedEvents++;

        EventPenalties +=
            definition.FailurePenalty;

        Debug.Log(
            $"Penalización de evento: " +
            $"-${definition.FailurePenalty}. " +
            $"Recaudación actual: ${TotalRevenue}"
        );
    }
}