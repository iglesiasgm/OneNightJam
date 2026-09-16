using TMPro;
using UnityEngine;

public class EconomyHUD : MonoBehaviour
{
    [SerializeField]
    private LevelEconomyManager economyManager;

    [SerializeField]
    private TMP_Text revenueText;

    private int lastDisplayedRevenue =
        int.MinValue;

    private void Start()
    {
        UpdateRevenueText();
    }

    private void Update()
    {
        if (economyManager == null)
            return;

        if (
            economyManager.TotalRevenue !=
            lastDisplayedRevenue
        )
        {
            UpdateRevenueText();
        }
    }

    private void UpdateRevenueText()
    {
        if (
            economyManager == null ||
            revenueText == null
        )
        {
            return;
        }

        int revenue =
            economyManager.TotalRevenue;

        revenueText.text =
            $"Recaudación: ${revenue}";

        lastDisplayedRevenue =
            revenue;
    }
}