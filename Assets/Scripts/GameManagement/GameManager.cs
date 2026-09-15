using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("Game Options References")]
    [SerializeField] private GameOptions gameOptions;
    
    [Header("System References")]
    [SerializeField] private TrainController trainController;
    [SerializeField] private WeatherManager weatherManager;
    [SerializeField] private GameClock gameClock;
    
    [Header("References")]
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private GameObject controlledTrain;

    [Header("Game Over Delays")]
    [SerializeField] private float missedStationDelay = 0.5f;
    [SerializeField] private float derailmentDelay = 2.5f;
    [SerializeField] private float roadCollisionDelay = 0.3f;

    public GameState CurrentGameState { get; private set; }
    
    private InputActions controls;

    private void Awake()
    {
        controls = new InputActions();

        trainController.SetControls(controls);
    }

    private void Start()
    {
        CurrentGameState = GameState.InGame;

        if (gameOptions)
        {
            weatherManager.Initialize(gameOptions);
            gameClock.Initialize(gameOptions);
        }

        controls.Enable();
    }

    private void StartGameOver(float delay)
    {
        if (CurrentGameState != GameState.InGame) return;

        CurrentGameState = GameState.GameOver;
        controls.Player.Disable();
        trainController.StopTrain();

        gameOverUI.ShowGameOver(delay);
    }
    
    public void GameOverDerailment()
    {
        if (controlledTrain != null) Destroy(controlledTrain);

        StartGameOver(derailmentDelay);
    }

    public void GameOverRoadCollision()
    {
        StartGameOver(roadCollisionDelay);
    }
    
    public void GameOverMissedStation()
    {
        StartGameOver(missedStationDelay);
    }
}

public enum GameState
{
    PreGame,
    InGame,
    GameOver
}