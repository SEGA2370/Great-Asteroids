using UnityEngine;

public class RoundManager : MonoBehaviour
{
    public static RoundManager Instance { get; private set; }

    [Header("Round Settings")]
    [SerializeField] private int roundsPerLevel = 5;
    private int currentRound = 1;
    private int currentLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void StartNewLevel()
    {
        currentLevel++;
        currentRound = 1;

        ResetGameSystems();
    }

    public void StartNextRound()
    {
        if (currentRound >= roundsPerLevel)
        {
            StartNewLevel();
        }
        else
        {
            currentRound++;
            ResetGameSystems();
        }
    }

    public void ResetGameSystems()
    {
        // Reset timers
        TimerManager.Instance?.ClearAllTimers();

        // Reset physics calculations
        ResetPhysics();

        // Reset game objects (e.g., spawners, player, enemies, etc.)
        ResetGameplayObjects();

        // Reset game state
        GameManager.Instance?.SetGameState(GameState.StartRound);
    }

    private void ResetPhysics()
    {
        // Reset physics time scale
        Time.timeScale = 1;

        // Temporarily set simulation mode to Script
        var previousSimulationMode = Physics2D.simulationMode;
        Physics2D.simulationMode = SimulationMode2D.Script;

        // Simulate a zero-time step to clear active physics calculations
        Physics2D.Simulate(0f);

        // Restore the original simulation mode
        Physics2D.simulationMode = previousSimulationMode;
    }

    private void ResetGameplayObjects()
    {
        // Reset asteroid spawner
        AsteroidSpawner.Instance?.ResetSpawner();

        // Reset enemy ship spawner
        EnemyShipSpawner.Instance?.ResetSpawner();

        // Reset player ship position and state
        if (GameManager.Instance?.PlayerShip != null)
        {
            GameManager.Instance.PlayerShip.ResetShipToStartPosition();
            GameManager.Instance.PlayerShip.ReviveShip();
        }
    }
}
