using UnityEngine;
using UnityEngine.InputSystem;

public class TrainController : MonoBehaviour
{
    [SerializeField] private TrainSplineFollower trainSplineFollower;
    [SerializeField] private float accelerationRate = 5f;
    [SerializeField] private float maxSpeed = 20f;
    [SerializeField] private float minSpeed = 0f;

    private InputActions controls;
    [SerializeField] private float currentSpeed = 0f;
    [SerializeField] private float inputValue = 0f;
    [SerializeField] private float currentAcceleration = 0f;

    private void OnEnable()
    {
        controls.Player.Acceleration.performed += OnAcceleration;
        controls.Player.Acceleration.canceled += OnAcceleration;
    }

    private void OnDisable()
    {
        controls.Disable();
    }
    
    private void OnAcceleration(InputAction.CallbackContext ctx)
    {
        inputValue = ctx.ReadValue<float>();
    }
    private void Update()
    {
        float previousSpeed = currentSpeed;

        currentSpeed += inputValue * accelerationRate * Time.deltaTime;

        currentSpeed = Mathf.Clamp(
            currentSpeed,
            minSpeed,
            maxSpeed
        );

        if (Time.deltaTime > 0f)
        {
            currentAcceleration =
                (currentSpeed - previousSpeed) /
                Time.deltaTime;
        }
        else
        {
            currentAcceleration = 0f;
        }

        trainSplineFollower.SetSpeed(currentSpeed);
    }
    public void SetControls(InputActions controls)
    {
        this.controls = controls;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public float GetCurrentAcceleration()
    {
        return currentAcceleration;
    }

    public void StopTrain()
    {
        inputValue = 0f;
        currentSpeed = 0f;

        trainSplineFollower.SetSpeed(0f);
    }
}