using System.Collections;
using TMPro;
using UnityEngine;

public class ServiceResultHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField]
    private LevelResultManager resultManager;

    [Header("Panel")]
    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform panelTransform;

    [Header("Texts")]
    [SerializeField]
    private TMP_Text passengersText;

    [SerializeField]
    private TMP_Text passengerRevenueText;

    [SerializeField]
    private TMP_Text successfulEventsText;

    [SerializeField]
    private TMP_Text eventRewardsText;

    [SerializeField]
    private TMP_Text failedEventsText;

    [SerializeField]
    private TMP_Text eventPenaltiesText;

    [SerializeField]
    private TMP_Text totalRevenueText;

    [SerializeField]
    private TMP_Text ministryBalanceText;

    [Header("Animation")]
    [SerializeField]
    private float fadeDuration = 0.5f;

    [SerializeField]
    private float startScale = 0.9f;

    private void Awake()
    {
        HideImmediate();
    }

    private void OnEnable()
    {
        if (resultManager != null)
        {
            resultManager.OnServiceFinished +=
                HandleServiceFinished;
        }
    }

    private void OnDisable()
    {
        if (resultManager != null)
        {
            resultManager.OnServiceFinished -=
                HandleServiceFinished;
        }
    }

    private void HandleServiceFinished(
        ServiceResultData result
    )
    {
        if (result == null)
            return;

        passengersText.text =
            $"Pasajeros transportados: " +
            $"{result.passengersTransported}";

        passengerRevenueText.text =
            $"Ingresos por boletos: " +
            $"{FormatMoney(result.passengerRevenue)}";

        successfulEventsText.text =
            $"Eventos atendidos: " +
            $"{result.successfulEvents}";

        eventRewardsText.text =
            $"Bonificaciones: " +
            $"{FormatMoney(result.eventRewards)}";

        failedEventsText.text =
            $"Eventos fallidos: " +
            $"{result.failedEvents}";

        eventPenaltiesText.text =
            $"Penalizaciones: " +
            $"-${result.eventPenalties}";

        totalRevenueText.text =
            $"RESULTADO: " +
            $"{FormatMoney(result.totalRevenue)}";

        ministryBalanceText.text =
            $"Ministerio: " +
            $"{FormatMoney(result.ministryBalanceBefore)} " +
            $"→ " +
            $"{FormatMoney(result.ministryBalanceAfter)}";

        StartCoroutine(
            ShowRoutine()
        );
    }

    private IEnumerator ShowRoutine()
    {
        canvasGroup.blocksRaycasts =
            true;

        panelTransform.localScale =
            Vector3.one *
            startScale;

        float elapsed = 0f;

        while (
            elapsed <
            fadeDuration
        )
        {
            elapsed +=
                Time.unscaledDeltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            canvasGroup.alpha =
                smoothT;

            panelTransform.localScale =
                Vector3.Lerp(
                    Vector3.one *
                    startScale,
                    Vector3.one,
                    smoothT
                );

            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;

        panelTransform.localScale =
            Vector3.one;
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
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