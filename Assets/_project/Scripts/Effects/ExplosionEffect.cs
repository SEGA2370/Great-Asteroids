using UnityEngine;
using UnityEngine.Pool;
using System.Collections;

public class ExplosionEffect : MonoBehaviour
{
    [SerializeField] private float _explosionVolume = 0.1f;

    private ParticleSystem _particleSystem;

    // Pool reference
    public IObjectPool<ExplosionEffect> Pool { get; set; }

    private void Awake()
    {
        _particleSystem = GetComponent<ParticleSystem>();

        if (_particleSystem == null)
        {
            Debug.LogError("ExplosionEffect: Missing ParticleSystem.");
        }
    }

    private void OnEnable()
    {
        if (_particleSystem == null) return;

        // Reset and play the particle system
        _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        _particleSystem.Play();

        // Play sound safely
        if (SfxManager.Instance != null)
        {
            SfxManager.Instance.PlayClip(SoundEffectsClip.Explosion, _explosionVolume);
        }

        // Schedule pool release
        float lifetime = _particleSystem.main.duration + _particleSystem.main.startLifetime.constant;
        StartCoroutine(ReleaseAfter(lifetime));
    }

    private IEnumerator ReleaseAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        EventBus.Instance?.Raise(new ExplosionCompletedEvent(this));
        Pool?.Release(this);
    }
}
