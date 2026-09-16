using System;
using UnityEngine;

[CreateAssetMenu(
    fileName = "MinistryWallet",
    menuName = "Tram Game/Ministry Wallet"
)]
public class MinistryWallet : ScriptableObject
{
    [Header("Initial Balance")]
    [SerializeField] private int initialBalance = 0;

    [Header("Runtime")]
    [SerializeField] private int currentBalance;

    [NonSerialized]
    private bool runtimeInitialized = false;

    public int CurrentBalance =>
        currentBalance;

    public event Action<int> OnBalanceChanged;

    private void OnEnable()
    {
        if (runtimeInitialized)
            return;

        currentBalance =
            initialBalance;

        runtimeInitialized = true;
    }

    public void AddMoney(
        int amount
    )
    {
        currentBalance += amount;

        OnBalanceChanged?.Invoke(
            currentBalance
        );

        Debug.Log(
            $"Ministerio: {FormatMoney(currentBalance)}"
        );
    }

    public bool CanAfford(
        int amount
    )
    {
        return currentBalance >= amount;
    }

    public void SpendMoney(
        int amount
    )
    {
        currentBalance -= amount;

        OnBalanceChanged?.Invoke(
            currentBalance
        );
    }

    public void ResetWallet()
    {
        currentBalance =
            initialBalance;

        OnBalanceChanged?.Invoke(
            currentBalance
        );
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