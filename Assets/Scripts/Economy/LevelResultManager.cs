using System;
using UnityEngine;

public class LevelResultManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private LevelEconomyManager economyManager;

    [SerializeField]
    private MinistryWallet ministryWallet;

    [Header("Behaviour")]
    [SerializeField]
    private bool pauseGameWhenFinished = true;

    public bool IsFinished
    {
        get;
        private set;
    }

    public ServiceResultData LastResult
    {
        get;
        private set;
    }

    public event Action<ServiceResultData>
        OnServiceFinished;

    public void FinishService()
    {
        if (IsFinished)
            return;

        if (
            economyManager == null ||
            ministryWallet == null
        )
        {
            Debug.LogError(
                "LevelResultManager: faltan referencias."
            );

            return;
        }

        IsFinished = true;

        int balanceBefore =
            ministryWallet.CurrentBalance;

        ServiceResultData result =
            new ServiceResultData
            {
                passengersTransported =
                    economyManager
                        .PassengersTransported,

                passengerRevenue =
                    economyManager
                        .PassengerRevenue,

                successfulEvents =
                    economyManager
                        .SuccessfulEvents,

                eventRewards =
                    economyManager
                        .EventRewards,

                failedEvents =
                    economyManager
                        .FailedEvents,

                eventPenalties =
                    economyManager
                        .EventPenalties,

                totalRevenue =
                    economyManager
                        .TotalRevenue,

                ministryBalanceBefore =
                    balanceBefore
            };

        ministryWallet.AddMoney(
            result.totalRevenue
        );

        result.ministryBalanceAfter =
            ministryWallet.CurrentBalance;

        LastResult =
            result;

        Debug.Log(
            $"SERVICIO FINALIZADO | " +
            $"Resultado: {FormatMoney(result.totalRevenue)} | " +
            $"Ministerio: {FormatMoney(result.ministryBalanceAfter)}"
        );

        OnServiceFinished?.Invoke(
            result
        );

        if (pauseGameWhenFinished)
        {
            Time.timeScale = 0f;
        }
    }

    private string FormatMoney(
        int amount
    )
    {
        if (amount < 0)
        {
            return $"-${Mathf.Abs(amount)}";
        }

        return $"${amount}";
    }
}