using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
public class PlayerDirector : MonoBehaviour
{
    [SerializeField] private PlayerShip _playerShip;
    [SerializeField] private float _fireDelay = 0.15f;
    [SerializeField] private float _invulnerabilityDuration = 3f;
    private PlayerInputBase _playerInput;
    private bool _fireEnabled, _isInvulnerable;
    private Timer _enableFireTimer, _cancelInvulnerabilityTimer;
    private void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        _playerInput = FindObjectOfType<PlayerTouchInput>();
#else
        _playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
#endif
    }
    private void OnEnable()
    {
        if (TimerManager.Instance == null)
        {
            return;
        }
        CreateTimers();
        EventBus.Instance?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    private void OnDisable()
    {
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        SafeReleaseTimers();
    }
    private void Update()
    {
        if (ShouldSkipUpdate()) return;
        CheckInvulnerability();
        HandleRotationInput();
        HandleThrustInput();
        HandleFireInput();
    }
    private bool ShouldSkipUpdate()
    {
        if (_playerInput == null || GameManager.Instance == null) return true;
        var gameState = GameManager.Instance?.CurrentGameState ?? GameState.GameOver;
        return !_playerShip.IsAlive || gameState == GameState.GameOver || gameState == GameState.RoundOver;
    }
    private void CreateTimers()
    {
        if (_enableFireTimer == null)
            _enableFireTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        if (_cancelInvulnerabilityTimer == null)
            _cancelInvulnerabilityTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
    }
    private void SafeReleaseTimers()
    {
        try
        {
            if (TimerManager.Instance == null)
            {
                return;
            }
            ReleaseTimer(ref _enableFireTimer);
            ReleaseTimer(ref _cancelInvulnerabilityTimer);
        }
        catch (System.Exception)
        {
            return;
        }
    }
    private void ReleaseTimer(ref Timer timer)
    {
        if (timer == null)
        {
            return;
        }
        if (TimerManager.Instance == null)
        {
            return;
        }
        TimerManager.Instance.ReleaseTimer<CountdownTimer>(timer);
        timer = null;
    }
    private void OnGameStateChanged(GameStateChangedEvent gameStateChangedEvent)
    {
        switch (gameStateChangedEvent.GameState)
        {
            case GameState.StartFirstRound:
            case GameState.StartRound:
            case GameState.ShipSpawned:
                ResetPlayerState();
                break;
            case GameState.PlayerDied:
            case GameState.GameOver:
                _playerShip.DisableShip();
                break;
        }
    }
    private void ResetPlayerState()
    {
        _playerShip.ResetShipToStartPosition();
        _playerShip.SetShipActiveState(true); // Ensures visibility and interaction.
        EnableInvulnerability();
        EnableFire();
    }
    private void CheckInvulnerability()
    {
        if (!_isInvulnerable || !_playerInput.AnyInputThisFrame) return;
        CancelInvulnerability();
    }
    private void EnableInvulnerability()
    {
        if (_isInvulnerable) return;
        _isInvulnerable = true;
        _cancelInvulnerabilityTimer.OnTimerStop += CancelInvulnerability;
        _cancelInvulnerabilityTimer.Start(_invulnerabilityDuration);
        _playerShip.EnableInvulnerability();
    }
    private void CancelInvulnerability()
    {
        if (!_isInvulnerable) return;
        _isInvulnerable = false;
        _cancelInvulnerabilityTimer.OnTimerStop -= CancelInvulnerability;
        _cancelInvulnerabilityTimer.Stop();
        _playerShip.CancelInvulnerability();
    }
    private void HandleRotationInput()
    {
        float rotationInput = _playerInput?.GetRotationInput() ?? 0f;
        if (Mathf.Approximately(rotationInput, 0f)) return;
        _playerShip.Rotate(rotationInput);
    }
    private void HandleThrustInput()
    {
        _playerShip.SetThrust(_playerInput?.GetThrustInput() ?? false);
    }
    private void EnableFire()
    {
        if (_fireEnabled) return; // Avoid overlapping
        _fireEnabled = true;
    }
    private void DisableFire()
    {
        if (!_fireEnabled) return; // Ensure it only disables when active
        _fireEnabled = false;
        _enableFireTimer.OnTimerStop += EnableFire;
        if (!_enableFireTimer.IsRunning) // Check if already running
            _enableFireTimer.Start(_fireDelay);
    }
    private void HandleFireInput()
    {
        if (!_fireEnabled || !(_playerInput?.GetFireInput() ?? false)) return;
        DisableFire();
        _playerShip.FireBullet();
    }
}