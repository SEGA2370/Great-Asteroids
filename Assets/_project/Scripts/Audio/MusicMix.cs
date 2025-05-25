using UnityEngine;
using UnityEngine.Audio;

[CreateAssetMenu(fileName = "Assets/_Project/Music/Music Mixes/MusicMix", menuName = "Music/Music Mix", order = 4)]
public class MusicMix : ScriptableObject
{
    [SerializeField] private AudioMixerGroup _mixerGroup;
    [SerializeField] private AudioMixerSnapshot _audioMixerSnapshot;

    private AudioSource _audioSource;
    private GameObject _audioSourceGameObject;

    public void Initialize()
    {
        if (_audioSource != null) return; // Prevent reinitialization

        if (MusicManager.Instance == null)
        {
            Debug.LogWarning("MusicManager.Instance is null. AudioSource not initialized.");
            return;
        }

        _audioSourceGameObject = new GameObject($"{name} Audio Source");
        _audioSourceGameObject.transform.SetParent(MusicManager.Instance.transform);
        _audioSource = _audioSourceGameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 1f;
        _audioSource.outputAudioMixerGroup = _mixerGroup;
    }

    public void PlayClip(AudioClip clip)
    {
        if (_audioSource == null)
        {
            Debug.LogWarning("AudioSource is not initialized.");
            return;
        }

        if (clip == null)
        {
            Debug.LogWarning("AudioClip is null. Cannot play.");
            return;
        }

        _audioSource.clip = clip;
        _audioSource.Play();
        _audioMixerSnapshot?.TransitionTo(1f);
    }

    public void Stop()
    {
        if (_audioSource == null) return;
        _audioSource.Stop();
    }

    public void Cleanup()
    {
        if (_audioSourceGameObject != null)
        {
            Destroy(_audioSourceGameObject);
            _audioSourceGameObject = null;
            _audioSource = null;
        }
    }
}
