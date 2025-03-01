using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Pool;
public class AsteroidSpawner : MonoBehaviour
{
    [SerializeField] private Asteroid[] _smallAsteroidPrefabs;
    [SerializeField] private Asteroid[] _mediumAsteroidPrefabs;
    [SerializeField] private Asteroid[] _largeAsteroidPrefabs;
    [SerializeField] private int _asteroidsToSpawn = 3, _maxAsteroids = 12;
    [SerializeField] private float _minSpawnDistanceFromPlayer = 2f;
    private readonly Dictionary<AsteroidSize, IObjectPool<Asteroid>> _asteroidPools = new();
    private readonly List<Asteroid> _asteroids = new();
    private bool _isSpawning = false;
    public static AsteroidSpawner Instance { get; private set; }
    private int SpawnCount => Mathf.Min(_asteroidsToSpawn + (GameManager.Instance?.Round ?? 1) - 1, _maxAsteroids);
    public int ActiveAsteroidsCount => _asteroids.Count;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EventBus.Instance?.Subscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    private void OnDestroy()
    {
        EventBus.Instance?.Unsubscribe<GameStateChangedEvent>(OnGameStateChanged);
    }
    public List<Asteroid> GetActiveAsteroids()
    {
        return _asteroids.Where(a => a != null && a.gameObject.activeInHierarchy).ToList();
    }
    public void RegisterAsteroid(Asteroid asteroid)
    {
        if (!_asteroids.Contains(asteroid))
        {
            _asteroids.Add(asteroid);
        }
    }
    public void UnregisterAsteroid(Asteroid asteroid)
    {
        _asteroids.Remove(asteroid);
    }
    public void StartSpawning()
    {
        if (_isSpawning)
        {
            return;
        }

        if (ActiveAsteroidsCount > 0)
        {
            return;
        }

        _isSpawning = true;

        for (int i = 0; i < SpawnCount; i++)
        {
            var asteroid = GetPool(AsteroidSize.Large).Get();
            if (!asteroid) continue;

            var spawnPoint = GetRandomSpawnPoint();
            asteroid.Initialize(this, spawnPoint);
            RegisterAsteroid(asteroid);
        }

        _isSpawning = false;
    }
    private void OnGameStateChanged(GameStateChangedEvent gameState)
    {
        if (gameState.GameState == GameState.StartFirstRound || gameState.GameState == GameState.StartRound)
        {
            SpawnAsteroids();
        }
    }
    private void SpawnAsteroids()
    {
        if (_isSpawning || ActiveAsteroidsCount > 0) return;
        _isSpawning = true;
        var pool = GetPool(AsteroidSize.Large);
        for (int i = 0; i < SpawnCount; i++)
        {
            var asteroid = pool.Get();
            if (!asteroid) continue;
            var spawnPoint = GetRandomSpawnPoint();
            asteroid.Initialize(this, spawnPoint);
            RegisterAsteroid(asteroid);
        }
        _isSpawning = false;
    }
    private Vector3 GetRandomSpawnPoint()
    {
        var playerPosition = GameManager.Instance?.PlayerShip?.transform.position ?? Vector3.zero;
        Vector3 spawnPoint;
        do
        {
            spawnPoint = ViewportHelper.Instance?.GetRandomVisiblePosition() ?? Vector3.zero;
        } while (Vector3.Distance(spawnPoint, playerPosition) < _minSpawnDistanceFromPlayer);
        return spawnPoint;
    }
    public void DestroyAsteroid(Asteroid asteroid, Vector3 position)
    {
        if (ExplosionSpawner.Instance == null)
        {
            return;
        }
        ExplosionSpawner.Instance.SpawnExplosion(position);
        SplitAsteroid(asteroid);
        ReleaseAsteroidToPool(asteroid);
        if (GetActiveAsteroids().Count == 0)
        {
            GameManager.Instance?.RoundOver();
        }
    }
    private void SplitAsteroid(Asteroid asteroid)
    {
        if (asteroid.Settings.Size == AsteroidSize.Small) return;
        var pool = GetPool(asteroid.Settings.Size - 1);
        for (int i = 0; i < 2; i++)
        {
            if (_asteroids.Count >= _maxAsteroids) break;
            var newAsteroid = pool.Get();
            if (!newAsteroid) continue;
            newAsteroid.Initialize(this, asteroid.transform.position);
            RegisterAsteroid(newAsteroid);
        }
    }
    private void ReleaseAsteroidToPool(Asteroid asteroid)
    {
        UnregisterAsteroid(asteroid);
        asteroid.gameObject.SetActive(false);
        GetPool(asteroid.Settings.Size).Release(asteroid);
    }
    public void ResetSpawner()
    {
        ReleaseAllAsteroids();
        foreach (var pool in _asteroidPools.Values)
        {
            pool.Clear();
        }
        _isSpawning = false;
    }
    public void ReleaseAllAsteroids()
    {
        foreach (var asteroid in _asteroids)
        {
            asteroid.gameObject.SetActive(false);
            GetPool(asteroid.Settings.Size).Release(asteroid);
        }
        _asteroids.Clear();
    }
    private IObjectPool<Asteroid> GetPool(AsteroidSize size)
    {
        if (_asteroidPools.TryGetValue(size, out var pool)) return pool;
        pool = new ObjectPool<Asteroid>(
            () => InstantiateAsteroid(size),
            asteroid => asteroid.gameObject.SetActive(true),
            asteroid => asteroid.gameObject.SetActive(false),
            asteroid => Destroy(asteroid.gameObject)
        );
        _asteroidPools.Add(size, pool);
        return pool;
    }
    private Asteroid InstantiateAsteroid(AsteroidSize size)
    {
        var prefab = size switch
        {
            AsteroidSize.Small => _smallAsteroidPrefabs[UnityEngine.Random.Range(0, _smallAsteroidPrefabs.Length)],
            AsteroidSize.Medium => _mediumAsteroidPrefabs[UnityEngine.Random.Range(0, _mediumAsteroidPrefabs.Length)],
            AsteroidSize.Large => _largeAsteroidPrefabs[UnityEngine.Random.Range(0, _largeAsteroidPrefabs.Length)],
            _ => null
        };
        if (!prefab)
        {
            return null;
        }
        var asteroid = Instantiate(prefab, transform);
        asteroid.gameObject.SetActive(false);
        return asteroid;
    }
}