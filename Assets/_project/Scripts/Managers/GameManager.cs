using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : SingletonMonoBehaviour<GameManager>
{
    [SerializeField] private float _spawnShipDelayTime = 2f;
    [SerializeField] private int _pointsForExtraLife = 10000;

    private List<EnemyShip> _enemyShips = new();
    private List<FastEnemyShip> _fastEnemyShips = new();
    private PlayerShip _playerShip;

    public int Round { get; private set; }
    public int Score { get; private set; }
    public int HighScore { get; private set; }
    public PlayerShip PlayerShip => _playerShip;

    private GameState _gameState = GameState.StartGame;
    public GameState CurrentGameState => _gameState;

    private int Lives { get; set; }
    private int _nextExtraLifeScore;
    private Timer _nextRoundTimer, _spawnShipTimer;

    protected override void Awake()
    {
        base.Awake();
 //       DontDestroyOnLoad(gameObject); // Only keep GameManager if necessary
        SceneManager.sceneLoaded += OnSceneLoaded;
        _playerShip = FindObjectOfType<PlayerShip>();
    }

    private void Start()
    {
        HighScore = SettingsManager.Instance.GetSetting<int>("HighScore", 0);
        StartGame();
    }
    protected override void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ResetSceneState(); // Ensure the scene state is reset, including the player.
    }
    private void ResetSceneState()
    {
        DestroyAllActiveEnemies();
        ReleaseTimers();

        // Reinitialize PlayerShip
        _playerShip = FindObjectOfType<PlayerShip>();
        if (_playerShip != null)
        {
            _playerShip.ResetShipToStartPosition();
            _playerShip.ReviveShip();
        }

        InitializeGameStats(); // Reset game stats
    }
    public void RestartGame()
    {
        // Reset spawners
        AsteroidSpawner.Instance?.ResetSpawner();
        EnemyShipSpawner.Instance?.ResetSpawner();

        // Destroy all active enemies and reset player
        DestroyAllActiveEnemies();
        ReleaseTimers();

        if (_playerShip != null)
        {
            _playerShip.AllowPositionReset(true);
            _playerShip.ResetShipToStartPosition();
            _playerShip.ReviveShip();
        }

        SetGameState(GameState.StartGame);
        StartGame();
    }

    private void StartGame()
    {
        InitializeGameStats();
        MusicManager.Instance.PlayMusic("Game");
        _playerShip.ReviveShip();

        AsteroidSpawner.Instance?.StartSpawning();
        EnemyShipSpawner.Instance?.EnableSpawning(true);

        StartFirstRound();
    }

    private void InitializeGameStats()
    {
        Lives = 3;
        Score = 0;
        Round = 0;
        _nextExtraLifeScore = _pointsForExtraLife;
    }

    private void StartFirstRound()
    {
        if (_gameState == GameState.StartFirstRound) return;
        CreateTimers();
        IncrementRound();
        AddPoints(0);
        SetGameState(GameState.StartFirstRound);
        StartSpawnShipTimer();
    }

    private void IncrementRound()
    {
        Round++;

        //// Check if round 5 is finished
        //if (Round > 5)
        //{
        //    LoadNextScene();
        //}
    }

    private void LoadNextScene()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            SceneManager.LoadScene(1);
        }
    }

    public void RoundOver()
    {
        if (_gameState == GameState.RoundOver || AsteroidSpawner.Instance.ActiveAsteroidsCount > 0) return;
        SetGameState(GameState.RoundOver);
        StartNextRoundTimer();
    }

    public void PlayerDied()
    {
       if (EventBus.Instance != null)
    {
        EventBus.Instance.Raise(new StopAllMusicEvent());
    }

    if (_playerShip != null)
    {
        _playerShip.AllowPositionReset(true);
    }
    else
    {
        return;
    }

    if (Lives > 0)
    {
        HandlePlayerDeath();

        if (AsteroidSpawner.Instance != null && AsteroidSpawner.Instance.ActiveAsteroidsCount == 0)
        {
            AsteroidSpawner.Instance.StartSpawning();
        }
    }
    else
    {
        GameOver();
    }
    }

    private void HandlePlayerDeath()
    {
        Lives--;
        EventBus.Instance.Raise(new PlayerLivesChangedEvent(Lives));
        SetGameState(GameState.PlayerDied);
        StartSpawnShipTimer();
    }

    private void DestroyAllActiveEnemies()
    {
        foreach (var enemy in _enemyShips)
        {
            if (enemy != null && enemy.gameObject.activeInHierarchy)
            {
                Destroy(enemy.gameObject);
            }
        }
        _enemyShips.Clear();

        foreach (var fastEnemy in _fastEnemyShips)
        {
            if (fastEnemy != null && fastEnemy.gameObject.activeInHierarchy)
            {
                Destroy(fastEnemy.gameObject);
            }
        }
        _fastEnemyShips.Clear();
    }

    private void GameOver()
    {
        ReleaseTimers();
        SetGameState(GameState.GameOver);
        EventBus.Instance.Raise(new PlayMusicEvent("GameOver"));
        DestroyAllActiveEnemies();
        EnemyShipSpawner.Instance.DestroyAllShips();
        ResetHighScore();
    }

    private void ResetHighScore()
    {
        SettingsManager.Instance.SetSetting("HighScore", HighScore.ToString(), true);
    }

    private void StartNextRoundTimer()
    {
        _nextRoundTimer?.Start(3f);
    }

    private void StartSpawnShipTimer()
    {
        _spawnShipTimer?.Start(_spawnShipDelayTime);
    }

    public void AddPoints(int points)
    {
        Score += points;
        UpdateHighScore();
        EventBus.Instance.Raise(new ScoreChangedEvent(Score, HighScore));
        CheckForExtraLife();
    }

    private void UpdateHighScore()
    {
        if (Score > HighScore)
        {
            HighScore = Score;
        }
    }

    private void CheckForExtraLife()
    {
        if (Score >= _nextExtraLifeScore)
        {
            _nextExtraLifeScore += _pointsForExtraLife;
            SfxManager.Instance.PlayClip(SoundEffectsClip.ExtraLife);
            EventBus.Instance.Raise(new PlayerLivesChangedEvent(++Lives));
        }
    }

    private void CreateTimers()
    {
        if (_nextRoundTimer == null) // Reuse timer if already created
            _nextRoundTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        if (_spawnShipTimer == null)
            _spawnShipTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();

        _nextRoundTimer.OnTimerStop += StartNextRound;
        _spawnShipTimer.OnTimerStop += SpawnShip;
    }

    private void ReleaseTimers()
    {
        ReleaseTimer(ref _nextRoundTimer);
        ReleaseTimer(ref _spawnShipTimer);
    }

    private void ReleaseTimer(ref Timer timer)
    {
        if (timer == null) return; // Ensure no redundant releases
        timer.OnTimerStop = null; // Remove all callbacks
        TimerManager.Instance.ReleaseTimer<CountdownTimer>(timer);
        timer = null;
    }

    private void StartNextRound()
    {
        if (_gameState != GameState.RoundOver) return;

        IncrementRound();
        SetGameState(GameState.StartRound);
        ResetPlayerShip();

        AsteroidSpawner.Instance.StartSpawning();
    }

    private void SpawnShip()
    {
        if (_gameState != GameState.PlayerDied) return;
        _playerShip.ReviveShip();
        SetGameState(GameState.ShipSpawned);
        ResetPlayerShip();
        EventBus.Instance.Raise(new PlayMusicEvent("Game"));

        if (AsteroidSpawner.Instance.ActiveAsteroidsCount == 0)
        {
            AsteroidSpawner.Instance.StartSpawning();
        }
    }

    private void ResetPlayerShip()
    {
        _playerShip.AllowPositionReset(false);
        _playerShip.ResetShipToStartPosition();
        _playerShip.EnableInvulnerability();
    }

    public void SetGameState(GameState gameState)
    {
        if (_gameState == gameState || (_gameState == GameState.GameOver && gameState != GameState.StartGame)) return;
        _gameState = gameState;
        EventBus.Instance.Raise(new GameStateChangedEvent(_gameState));
    }

    //public void PauseGame()
    //{
    //    Time.timeScale = 0;
    //}

    //public void ResumeGame()
    //{
    //    Time.timeScale = 1;
    //}
}
public enum GameState
{
    StartGame,
    StartFirstRound,
    StartRound,
    ShipSpawned,
    PlayerDied,
    RoundOver,
    GameOver,
}