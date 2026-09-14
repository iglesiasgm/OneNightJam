using TMPro;
using UnityEngine;

public class TrainHUD : MonoBehaviour
{
    [Header("Game References")]
    [SerializeField]
    private TrainController trainController;

    [SerializeField]
    private TrainStationLogic trainStationLogic;

    [SerializeField] private GameClock gameClock;

    [Header("HUD Texts")]
    [SerializeField]
    private TMP_Text speedText;

    [SerializeField]
    private TMP_Text accelerationText;

    [SerializeField]
    private TMP_Text nextStationText;

    [SerializeField]
    private TMP_Text distanceText;

    [SerializeField] private TMP_Text clockText;

    private void Update()
    {
        UpdateTrainData();
        UpdateStationData();
        UpdateClockData();
    }

    private void UpdateTrainData()
    {
        if (trainController == null)
            return;

        float speedMetersPerSecond =
            trainController.GetCurrentSpeed();

        float speedKilometersPerHour =
            speedMetersPerSecond * 3.6f;

        float acceleration =
            trainController.GetCurrentAcceleration();

        if (Mathf.Abs(acceleration) < 0.01f)
        {
            acceleration = 0f;
        }

        if (speedText != null)
        {
            speedText.text =
                $"Velocidad: " +
                $"{speedKilometersPerHour:0.0} km/h";
        }

        if (accelerationText != null)
        {
            if (acceleration > 0.01f)
            {
                accelerationText.text =
                    $"Aceleración: +{acceleration:0.0} m/s²";
            }
            else if (acceleration < -0.01f)
            {
                accelerationText.text =
                    $"Aceleración: -{Mathf.Abs(acceleration):0.0} m/s²";
            }
            else
            {
                accelerationText.text =
                    "Aceleración: 0.0 m/s²";
            }
        }
    }

    private void UpdateStationData()
    {
        if (trainStationLogic == null)
            return;

        string stationName =
            trainStationLogic
                .GetNextStationName();

        float remainingDistance =
            trainStationLogic
                .GetDistanceToNextStation();

        if (nextStationText != null)
        {
            nextStationText.text =
                $"Próxima estación: " +
                $"{stationName}";
        }

        if (distanceText != null)
        {
            int meters =
                Mathf.CeilToInt(
                    remainingDistance
                );

            distanceText.text =
                $"Distancia: " +
                $"{meters} m";
        }
    }

    private void UpdateClockData()
    {
        if(gameClock == null) return;
        if(clockText != null)
        {
            clockText.text = gameClock.CurrentTimeText;
        }
    }
}