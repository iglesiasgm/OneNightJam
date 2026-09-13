using UnityEngine;

public class TrainAudioController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrainController trainController;

    [SerializeField] private AudioSource runningAudioSource;
    [SerializeField] private AudioSource brakeAudioSource;

    [Header("Running sound")]
    [SerializeField] private float minSpeedForRunningSound = 0.01f;

    [Tooltip("Velocidad a partir de la cual el sonido alcanza su volumen máximo.")]
    [SerializeField] private float fullVolumeAtSpeed = 12f;

    [Range(0f, 1f)]
    [SerializeField] private float startingRunningVolume = 0.08f;

    [Range(0f, 1f)]
    [SerializeField] private float maxRunningVolume = 0.35f;

    [SerializeField] private float runningFadeSpeed = 3f;

    [Header("Running pitch")]
    [SerializeField] private float minimumRunningPitch = 0.6f;
    [SerializeField] private float maximumRunningPitch = 1.05f;

    [Header("Brakes")]
    [SerializeField] private float brakingAccelerationThreshold = -0.1f;

    [SerializeField] private float minSpeedForBrakeSound = 0.2f;

    [Range(0f, 1f)]
    [SerializeField] private float maxBrakeVolume = 0.9f;

    [SerializeField] private float brakeFadeSpeed = 3f;

    private void Start()
    {
        if (runningAudioSource != null)
        {
            runningAudioSource.volume = 0f;
        }

        if (brakeAudioSource != null)
        {
            brakeAudioSource.volume = 0f;
        }
    }

    private void LateUpdate()
    {
        if (trainController == null)
            return;

        float currentSpeed =
            trainController.GetCurrentSpeed();

        float currentAcceleration =
            trainController.GetCurrentAcceleration();

        UpdateRunningSound(
            currentSpeed, currentAcceleration
        );

        UpdateBrakeSound(
            currentSpeed,
            currentAcceleration
        );
    }

    private void UpdateRunningSound(
    float currentSpeed,
    float currentAcceleration
)
    {
        if (runningAudioSource == null)
            return;

        bool trainIsMoving =
            currentSpeed > minSpeedForRunningSound;

        bool trainIsStarting =
            currentAcceleration > 0.01f;

        bool shouldPlay =
            trainIsMoving ||
            trainIsStarting;

        if (
            shouldPlay &&
            !runningAudioSource.isPlaying
        )
        {
            runningAudioSource.volume =
                startingRunningVolume;

            runningAudioSource.pitch =
                minimumRunningPitch;

            runningAudioSource.Play();
        }

        float speedPercent =
            Mathf.InverseLerp(
                0f,
                fullVolumeAtSpeed,
                currentSpeed
            );

        float targetVolume =
            shouldPlay
            ? Mathf.Lerp(
                startingRunningVolume,
                maxRunningVolume,
                speedPercent
            )
            : 0f;

        runningAudioSource.volume =
            Mathf.MoveTowards(
                runningAudioSource.volume,
                targetVolume,
                runningFadeSpeed *
                Time.deltaTime
            );

        runningAudioSource.pitch =
            Mathf.Lerp(
                minimumRunningPitch,
                maximumRunningPitch,
                speedPercent
            );

        if (
            !shouldPlay &&
            runningAudioSource.isPlaying &&
            runningAudioSource.volume <= 0.001f
        )
        {
            runningAudioSource.Stop();
        }
    }

    private void UpdateBrakeSound(
        float currentSpeed,
        float currentAcceleration
    )
    {
        if (brakeAudioSource == null)
            return;

        bool isBraking =
            currentAcceleration <
            brakingAccelerationThreshold
            &&
            currentSpeed >
            minSpeedForBrakeSound;

        if (
            isBraking &&
            !brakeAudioSource.isPlaying
        )
        {
            brakeAudioSource.volume = 0f;
            brakeAudioSource.Play();
        }

        float targetVolume =
            isBraking
            ? maxBrakeVolume
            : 0f;

        brakeAudioSource.volume =
            Mathf.MoveTowards(
                brakeAudioSource.volume,
                targetVolume,
                brakeFadeSpeed *
                Time.deltaTime
            );

        if (
            !isBraking &&
            brakeAudioSource.isPlaying &&
            brakeAudioSource.volume <= 0.001f
        )
        {
            brakeAudioSource.Stop();
        }
    }
}