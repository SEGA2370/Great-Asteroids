using UnityEngine;
using UnityEngine.Audio;
[CreateAssetMenu(fileName = "Assets/_Project/Music/Music Mixes/MusicMix", menuName = "Music/Music Mix", order = 4)]
public class MusicMix : ScriptableObject
{
    [SerializeField] AudioMixerGroup _mixerGroup;
    [SerializeField] AudioMixerSnapshot _audioMixerSnapshot;
    AudioSource _audioSource;
    public void Initialize()
    {
        var audioSourceGameObject = new GameObject($"{name} Audio Source");
        audioSourceGameObject.transform.SetParent(MusicManager.Instance.transform);
        _audioSource = audioSourceGameObject.AddComponent<AudioSource>();
        _audioSource.loop = true;
        _audioSource.playOnAwake = false;
        _audioSource.spatialBlend = 0f;
        _audioSource.volume = 1f;
        _audioSource.outputAudioMixerGroup = _mixerGroup;
    }
    public void PlayClip(AudioClip clip)
    {
        _audioSource.clip = clip;
        _audioSource.Play();
        _audioMixerSnapshot.TransitionTo(1f);
    }
    public void Stop()
    {
        _audioSource.Stop();
    }
}