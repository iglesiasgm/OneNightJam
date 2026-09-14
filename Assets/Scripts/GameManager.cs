using UnityEngine;

public class GameManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TrainController trainController;
    [SerializeField] private GameOverUI gameOverUI;
    [SerializeField] private GameObject controlledTrain;

    [Header("Game Over Delays")]
    [SerializeField] private float missedStationDelay = 0.5f;
    [SerializeField] private float derailmentDelay = 2.5f;
    [SerializeField] private float roadCollisionDelay = 0.3f;

    public GameState currentGameState { get; private set; }

    private InputActions controls;

    private bool gameOverStarted = false;

    private void Awake()
    {
        controls = new InputActions();

        trainController.SetControls(
            controls
        );
    }

    private void Start()
    {
        currentGameState =
            GameState.PreGame;

        controls.Enable();
    }
    /// <summary>
    /// Métodos de GameOver para diferentes condiciones de derrota
    /// </summary>
    public void GameOverMissedStation()
    {
        StartGameOver(
            missedStationDelay
        );
    }

    public void GameOverDerailment()
    {

        if (controlledTrain != null)
        {
            Destroy(controlledTrain);
        }

        StartGameOver(
            derailmentDelay
        );
    }

    private void StartGameOver(
        float delay
    )
    {
        if (gameOverStarted)
            return;

        gameOverStarted = true;

        currentGameState =
            GameState.GameOver;

        controls.Player.Disable();

        trainController.StopTrain();

        gameOverUI.ShowGameOver(
            delay
        );
    }

    public void GameOverRoadCollision()
    {
        StartGameOver(roadCollisionDelay);
    }

    /////////////////////////////
}

public enum GameState
{
    PreGame,
    InGame,
    GameOver
}