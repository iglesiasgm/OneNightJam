using System.Collections;
using TMPro;
using UnityEngine;

public class PassengerHUD : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField]
    private TrainPassengerManager passengerManager;

    [SerializeField]
    private TrainStationLogic trainStationLogic;

    [Header("Permanent HUD")]
    [SerializeField]
    private TMP_Text passengerCountText;

    [Header("Station Popup")]
    [SerializeField]
    private CanvasGroup popupCanvasGroup;

    [SerializeField]
    private RectTransform popupRectTransform;

    [SerializeField]
    private TMP_Text disembarkedText;

    [SerializeField]
    private TMP_Text boardedText;

    [Header("Animation")]
    [SerializeField]
    private float fadeInDuration = 0.25f;

    [SerializeField]
    private float visibleDuration = 2f;

    [SerializeField]
    private float fadeOutDuration = 0.5f;

    [SerializeField]
    private float startScale = 0.85f;

    [SerializeField]
    private float verticalAnimationDistance = 20f;

    private Coroutine popupCoroutine;

    private Vector2 popupFinalPosition;

    private void Start()
    {
        if (popupRectTransform != null)
        {
            popupFinalPosition =
                popupRectTransform.anchoredPosition;
        }

        HidePopupImmediate();

        UpdatePassengerCount();
    }

    private void OnEnable()
    {
        if (trainStationLogic != null)
        {
            trainStationLogic
                .OnStationPassengerExchangeCompleted +=
                HandleStationPassengerExchange;
        }

        if (passengerManager != null)
        {
            passengerManager.OnPassengerBoarded +=
                UpdatePassengerCount;

            passengerManager.OnPassengersDisembarked +=
                HandlePassengersDisembarked;
        }
    }

    private void OnDisable()
    {
        if (trainStationLogic != null)
        {
            trainStationLogic
                .OnStationPassengerExchangeCompleted -=
                HandleStationPassengerExchange;
        }

        if (passengerManager != null)
        {
            passengerManager.OnPassengerBoarded -=
                UpdatePassengerCount;

            passengerManager.OnPassengersDisembarked -=
                HandlePassengersDisembarked;
        }
    }

    private void HandlePassengersDisembarked(
        int amount
    )
    {
        UpdatePassengerCount();
    }

    private void UpdatePassengerCount()
    {
        if (
            passengerManager == null ||
            passengerCountText == null
        )
        {
            return;
        }

        passengerCountText.text =
            $"Pasajeros: " +
            $"{passengerManager.PassengerCount} / " +
            $"{passengerManager.PassengerCapacity}";
    }

    private void HandleStationPassengerExchange(
        int disembarked,
        int boarded
    )
    {
        UpdatePassengerCount();

        if (disembarkedText != null)
        {
            disembarkedText.text =
                $"-{disembarked}";
        }

        if (boardedText != null)
        {
            boardedText.text =
                $"+{boarded}";
        }

        if (popupCoroutine != null)
        {
            StopCoroutine(
                popupCoroutine
            );
        }

        popupCoroutine =
            StartCoroutine(
                ShowPopupRoutine()
            );
    }

    private IEnumerator ShowPopupRoutine()
    {
        if (
            popupCanvasGroup == null ||
            popupRectTransform == null
        )
        {
            yield break;
        }

        popupCanvasGroup.alpha = 0f;

        popupRectTransform.localScale =
            Vector3.one *
            startScale;

        popupRectTransform.anchoredPosition =
            popupFinalPosition -
            new Vector2(
                0f,
                verticalAnimationDistance
            );

        float elapsed = 0f;

        while (
            elapsed <
            fadeInDuration
        )
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeInDuration
                );

            float smoothT =
                Mathf.SmoothStep(
                    0f,
                    1f,
                    t
                );

            popupCanvasGroup.alpha =
                smoothT;

            popupRectTransform.localScale =
                Vector3.Lerp(
                    Vector3.one *
                    startScale,
                    Vector3.one,
                    smoothT
                );

            popupRectTransform.anchoredPosition =
                Vector2.Lerp(
                    popupFinalPosition -
                    new Vector2(
                        0f,
                        verticalAnimationDistance
                    ),
                    popupFinalPosition,
                    smoothT
                );

            yield return null;
        }

        popupCanvasGroup.alpha = 1f;

        popupRectTransform.localScale =
            Vector3.one;

        popupRectTransform.anchoredPosition =
            popupFinalPosition;

        yield return
            new WaitForSeconds(
                visibleDuration
            );

        elapsed = 0f;

        while (
            elapsed <
            fadeOutDuration
        )
        {
            elapsed +=
                Time.deltaTime;

            float t =
                Mathf.Clamp01(
                    elapsed /
                    fadeOutDuration
                );

            popupCanvasGroup.alpha =
                1f - t;

            yield return null;
        }

        HidePopupImmediate();

        popupCoroutine = null;
    }

    private void HidePopupImmediate()
    {
        if (popupCanvasGroup != null)
        {
            popupCanvasGroup.alpha = 0f;
            popupCanvasGroup.interactable = false;
            popupCanvasGroup.blocksRaycasts = false;
        }

        if (popupRectTransform != null)
        {
            popupRectTransform.localScale =
                Vector3.one;

            popupRectTransform.anchoredPosition =
                popupFinalPosition;
        }
    }
}