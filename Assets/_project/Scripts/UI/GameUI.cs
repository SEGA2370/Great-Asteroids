using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
public class GameUI : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText, _highScoreText, _gameOverText, _playAgainText;
    [SerializeField] private Button _playAgainButton;
    [SerializeField] private Transform _playerLivesParent;
    [SerializeField] private UIButton _settingsButton;
    [SerializeField] private GameObject _playerTouchInput;
    private Timer _showPlayAgainPromptTimer;
    private bool _isGameOver;
    private void OnEnable()
    {
        SubscribeToEvents();
        _settingsButton?.Init(LoadSettingsScene);
        _playAgainButton?.onClick.AddListener(RestartGame);
        ResetUI();
        UpdatePlayerLives(3);
        _showPlayAgainPromptTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
#if UNITY_IOS || UNITY_ANDROID
        _playerTouchInput?.SetActive(true);
#else
        _playerTouchInput?.SetActive(false);
#endif
    }
    private void OnDisable()
    {
        UnsubscribeFromEvents();
        if (_showPlayAgainPromptTimer != null)
        {
            _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
            TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_showPlayAgainPromptTimer);
            _showPlayAgainPromptTimer = null; // Nullify after release to prevent reuse
        }
        if (_playAgainButton != null)
        {
            _playAgainButton.onClick.RemoveListener(RestartGame);
        }
        if (_settingsButton != null)
        {
            _settingsButton.Cleanup(); // If UIButton has a cleanup method, ensure it's called
        }
#if UNITY_IOS || UNITY_ANDROID
        _playerTouchInput?.SetActive(false); // Safeguard _playerTouchInput
#endif
    }
    private void SubscribeToEvents()
    {
        EventBus.Instance?.Subscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance?.Subscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    private void UnsubscribeFromEvents()
    {
        EventBus.Instance?.Unsubscribe<ScoreChangedEvent>(OnScoreChanged);
        EventBus.Instance?.Unsubscribe<PlayerLivesChangedEvent>(OnPlayerLivesChanged);
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    private void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        if (gameStateChangedEvent.GameState == GameState.GameOver)
        {
            HandleGameOver();
        }
        else
        {
            HandleGameReset();
        }
    }
    private void HandleGameOver()
    {
        _isGameOver = true;
        _gameOverText.enabled = true;
        _showPlayAgainPromptTimer.OnTimerStop += ShowPlayAgainPrompt;
        _showPlayAgainPromptTimer.Start(3f);
    }
    private void HandleGameReset()
    {
        _isGameOver = false;
        ResetUI();
    }
    private void ShowPlayAgainPrompt()
    {
        _playAgainButton?.gameObject.SetActive(true);
        _playAgainText.enabled = true;
        _showPlayAgainPromptTimer.OnTimerStop -= ShowPlayAgainPrompt;
        _showPlayAgainPromptTimer.Stop();
    }
    public void RestartGame()
    {
        if (!_isGameOver) return;
        ResetUI();
        _isGameOver = false;
        GameManager.Instance.RestartGame();
    }
    private void ResetUI()
    {
        _gameOverText.enabled = false;
        _playAgainText.enabled = false;
        _playAgainButton?.gameObject.SetActive(false);
    }
    private void OnScoreChanged(ScoreChangedEvent scoreChangedEvent)
    {
        _scoreText.text = scoreChangedEvent.Score.ToString();
        _highScoreText.text = scoreChangedEvent.HighScore.ToString();
    }
    private void OnPlayerLivesChanged(PlayerLivesChangedEvent playerLivesChangedEvent)
    {
        UpdatePlayerLives(playerLivesChangedEvent.Lives);
    }
    private void UpdatePlayerLives(int lives)
    {
        if (_playerLivesParent == null) return;
        for (var i = 0; i < _playerLivesParent.childCount; i++)
        {
            _playerLivesParent.GetChild(i).gameObject.SetActive(i < lives);
        }
    }
    private void LoadSettingsScene()
    {
        EventBus.Instance?.Subscribe<SettingsSceneClosedEvent>(ResumeGame);
        //GameManager.Instance.PauseGame();
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }
    private void ResumeGame(SettingsSceneClosedEvent _)
    {
        EventBus.Instance?.Unsubscribe<SettingsSceneClosedEvent>(ResumeGame);
//       GameManager.Instance.ResumeGame();
    }
}