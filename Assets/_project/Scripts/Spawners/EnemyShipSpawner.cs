using System.Collections.Generic;
using UnityEngine;
public class EnemyShipSpawner : SingletonMonoBehaviour<EnemyShipSpawner>
{
    [SerializeField] private EnemyShip[] _enemyShipPrefabs;
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private Transform[] _waypoints;

    [Header("Spawn Delay Settings")]
    [SerializeField] private float _subsequentSpawnDelay = 10f;
    [SerializeField] private float _minSpawnDelay = 5f;
    [SerializeField] private float _spawnDelayDecrement = 0.1f;

    private float _fastShipPercentage = 0.2f;
    private float _spawnDelay;
    private bool _enableSpawning;
    private readonly List<Vector3> _waypointsList = new();
    private Timer _spawnTimer;


    protected override void Awake()
    {
        base.Awake();
        if (_enemyShipPrefabs.Length < 2)
        {
            return;
        }
        InitializeSpawnTimer();
        EventBus.Instance.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    private void OnDisable()
    {
        DestroyAllShips();
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
        TimerManager.Instance?.ReleaseTimer<CountdownTimer>(_spawnTimer);
    }
    public void DestroyAllShips()
    {
        foreach (var ship in FindObjectsOfType<EnemyShip>())
        {
            if (ship != null)
            {
                ship.ReleaseFireTimer(); // Safely release associated timers
                Destroy(ship.gameObject);
            }
        }
    }
    private void InitializeSpawnTimer()
    {
        _spawnTimer = TimerManager.Instance.CreateTimer<CountdownTimer>();
        _spawnTimer.OnTimerStop += SpawnShip;
        _spawnDelay = _subsequentSpawnDelay;
    }
    private void OnGameStateChanged(GameStateChangedEvent gameState)
    {
        EnableSpawning(gameState.GameState != GameState.GameOver);
    }
    public void EnableSpawning(bool enable)
    {
        if (_enableSpawning == enable) return;
        if (enable)
        {
            _spawnTimer.Start(_spawnDelay);
        }
        else
        {
            _spawnTimer.Stop();
        }
        _enableSpawning = enable;
    }
    [ContextMenu("Spawn Ship")]
    private void SpawnShip()
    {
        if (!_enableSpawning) return;

        if (_spawnPoints == null || _spawnPoints.Length == 0)
        {
            return;
        }

        int spawnIndex = UnityEngine.Random.Range(0, _spawnPoints.Length);

        if (_spawnPoints[spawnIndex] == null)
        {
            return;
        }

        var shipPrefab = UnityEngine.Random.value < _fastShipPercentage
            ? _enemyShipPrefabs[1] // Fast
            : _enemyShipPrefabs[0]; // Slow

        var ship = Instantiate(shipPrefab);
        ship.Init(this, _spawnPoints[spawnIndex].position, GetRandomWaypoints(spawnIndex));

        _fastShipPercentage = Mathf.Min(1f, _fastShipPercentage + 0.05f);
        _spawnDelay = Mathf.Max(_minSpawnDelay, _spawnDelay - _spawnDelayDecrement);
        _spawnTimer.Start(_spawnDelay);
    }
    public void ResetSpawner()
    {
        // Stop spawning
        EnableSpawning(false);

        // Destroy all active enemy ships
        DestroyAllShips();

        // Reset spawn delay and fast ship spawn percentage
        _spawnDelay = _subsequentSpawnDelay;
        _fastShipPercentage = 0.2f;

        // Reset the timer (optional, if needed)
        if (_spawnTimer != null)
        {
            _spawnTimer.Stop();
            _spawnTimer.Start(_spawnDelay);
        }
    }
    private Vector3[] GetRandomWaypoints(int spawnPointIndex)
    {
        _waypointsList.Clear();
        var isLeftSide = _spawnPoints[spawnPointIndex].position.x < 0;
        _waypointsList.Add(isLeftSide
            ? _waypoints[spawnPointIndex].position
            : _waypoints[spawnPointIndex + 3].position);
        for (int i = 0; i < 2; i++)
        {
            var verticalIndex = UnityEngine.Random.Range(0, 3);
            var waypointIndex = isLeftSide ? i * 3 + verticalIndex : (1 - i) * 3 + verticalIndex;
            _waypointsList.Add(_waypoints[waypointIndex].position);
        }
        return _waypointsList.ToArray();
    }
    // Add the missing DespawnEnemyShip method
    public void DespawnEnemyShip(EnemyShip ship)
    {
        if (ship == null) return;

        // Release associated timers
        ship.ReleaseFireTimer();

        // Clean up any child objects or effects
        foreach (Transform child in ship.transform)
        {
            Destroy(child.gameObject);
        }

        // Destroy the ship game object
        Destroy(ship.gameObject);
    }
}