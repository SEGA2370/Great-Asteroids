using UnityEngine;
using UnityEngine.Audio;

public class AudioSettings : SingletonMonoBehaviour<AudioSettings>
{
    [SerializeField] private AudioMixer _audioMixer;

    public const string MasterVolumeKey = "MasterVolume";
    public const string MusicVolumeKey = "MusicVolume";
    public const string SfxVolumeKey = "SfxVolume";

    /// <summary>Loads the saved audio settings from PlayerPrefs and applies them to the mixer.</summary>
    public void LoadAudioSettings()
    {
        SetMixerVolume(MasterVolumeKey, SettingsManager.Instance.GetSetting(MasterVolumeKey, 0f));
        SetMixerVolume(MusicVolumeKey, SettingsManager.Instance.GetSetting(MusicVolumeKey, 0f));
        SetMixerVolume(SfxVolumeKey, SettingsManager.Instance.GetSetting(SfxVolumeKey, 0f));
    }

    /// <summary>Saves current volume values to PlayerPrefs and applies them.</summary>
    public void SaveAudioSettings(float masterVolume, float musicVolume, float sfxVolume)
    {
        SettingsManager.Instance.SetSetting(MasterVolumeKey, masterVolume);
        SettingsManager.Instance.SetSetting(MusicVolumeKey, musicVolume);
        SettingsManager.Instance.SetSetting(SfxVolumeKey, sfxVolume, true);

        SetMixerVolume(MasterVolumeKey, masterVolume);
        SetMixerVolume(MusicVolumeKey, musicVolume);
        SetMixerVolume(SfxVolumeKey, sfxVolume);
    }

    private void SetMixerVolume(string key, float value)
    {
        _audioMixer.SetFloat(key, value);
    }
}
