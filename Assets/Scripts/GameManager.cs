using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TrainController trainController;
    [SerializeField] private GameObject controlledTrain;

    public GameState currentGameState { get; private set; }

    private InputActions controls;
    
    private void Awake()
    {
        controls = new InputActions();
        trainController.SetControls(controls);
    }

    private void Start()
    {
        currentGameState = GameState.PreGame;
        
        controls.Enable();
    }

    public void GameOver()
    {
        currentGameState = GameState.GameOver;
        
        Destroy(controlledTrain);
        
        controls.Player.Disable();
    }
}

public enum GameState
{
    PreGame,
    InGame,
    GameOver
}