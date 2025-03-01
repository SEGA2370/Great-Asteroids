using UnityEngine;
using UnityEngine.Pool;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float _explosionVolume = 0.1f;
    private ParticleSystem _particleSystem;
    private float _completedTime;

    // Pool reference
    public IObjectPool<ExplosionEffect> Pool { get; set; }

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
        {
           return;
        }
    }

    private void OnEnable()
    {
        if (_particleSystem == null) return;

        SfxManager.Instance.PlayClip(SoundEffectsClip.Explosion, _explosionVolume);
        _particleSystem.Play();
        _completedTime = Time.time + _particleSystem.main.duration;
    }

    private void Update()
    {
        if (_particleSystem == null || !_particleSystem.isPlaying) return;

        if (Time.time >= _completedTime)
        {
            HandleExplosionCompletion();
        }
    }

    private void HandleExplosionCompletion()
    {
        // Raise an event if needed
        EventBus.Instance?.Raise(new ExplosionCompletedEvent(this));

        // Return to the pool instead of destroying
        Pool?.Release(this);
    }
}
