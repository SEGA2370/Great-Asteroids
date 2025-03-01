using UnityEngine;
using UnityEngine.Pool;

public class ExplosionSpawner : MonoBehaviour
{
    public static ExplosionSpawner Instance { get; private set; } // Singleton instance

    [SerializeField] private ExplosionEffect _explosionPrefab;
    private IObjectPool<ExplosionEffect> _explosionPool;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // Prevent multiple instances
            return;
        }

        Instance = this; // Assign this instance to the static property
        _explosionPool = new ObjectPool<ExplosionEffect>(
            CreateExplosion, OnGetExplosion, OnReleaseExplosion, OnDestroyExplosion
        );
    }

    private ExplosionEffect CreateExplosion()
    {
        var explosion = Instantiate(_explosionPrefab);
        explosion.Pool = _explosionPool;
        return explosion;
    }

    private void OnGetExplosion(ExplosionEffect explosion)
    {
        explosion.gameObject.SetActive(true);
    }

    private void OnReleaseExplosion(ExplosionEffect explosion)
    {
        explosion.gameObject.SetActive(false);
    }

    private void OnDestroyExplosion(ExplosionEffect explosion)
    {
        Destroy(explosion.gameObject);
    }

    public void SpawnExplosion(Vector3 position)
    {
        var explosion = _explosionPool.Get();
        explosion.transform.position = position;
    }
}
