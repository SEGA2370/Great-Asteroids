using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SfxManager : SingletonMonoBehaviour<SfxManager>
{
    [SerializeField] private List<AudioClip> _sfxClips = new();
    private AudioSource _audioSource;

    protected override void Awake()
    {
        base.Awake(); // Ensure base singleton setup
        _audioSource = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Plays a sound effect from the clip list.
    /// </summary>
    /// <param name="clip">The enum value of the sound effect.</param>
    /// <param name="volume">Optional volume (0.0 to 1.0).</param>
    public void PlayClip(SoundEffectsClip clip, float volume = 1f)
    {
        int index = (int)clip;
        if (index >= 0 && index < _sfxClips.Count)
        {
            var selectedClip = _sfxClips[index];
            if (selectedClip != null)
            {
                _audioSource.PlayOneShot(selectedClip, volume);
            }
            else
            {
                Debug.LogWarning($"SfxManager: AudioClip for {clip} is null.");
            }
        }
        else
        {
            Debug.LogWarning($"SfxManager: Invalid SFX index {index} for enum {clip}.");
        }
    }
}

public enum SoundEffectsClip
{
    PlayerBulletFire,
    EnemyBulletFire,
    Explosion,
    ExtraLife
}