using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class SettingsManager : SingletonMonoBehaviour<SettingsManager>
{
    private readonly Dictionary<string, string> _settings = new();
    private const string SettingsKey = "PreferenceKeys";

    protected override void Awake()
    {
        base.Awake();
        LoadSettings();
    }

    public string GetSetting(string key, string defaultValue)
    {
        if (!_settings.TryGetValue(key, out var value))
        {
            value = defaultValue;
            _settings[key] = value;
        }
        return value;
    }

    public int GetSetting<TInt32>(string key, int defaultValue)
    {
        return _settings.TryGetValue(key, out var value) && int.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public float GetSetting(string key, float defaultValue)
    {
        return _settings.TryGetValue(key, out var value) && float.TryParse(value, out var result)
            ? result
            : defaultValue;
    }

    public void SetSetting(string key, string value, bool save = false)
    {
        _settings[key] = value;
        if (save) SaveSettings();
    }

    public void SetSetting<T>(string key, T value, bool save = false)
    {
        SetSetting(key, value.ToString(), save);
    }

    private void LoadSettings()
    {
        var keysRaw = PlayerPrefs.GetString(SettingsKey, string.Empty);
        if (string.IsNullOrEmpty(keysRaw)) return;

        foreach (var key in keysRaw.Split(','))
        {
            if (!string.IsNullOrEmpty(key))
            {
                var value = PlayerPrefs.GetString(key, string.Empty);
                _settings[key] = value;
            }
        }
    }

    private void SaveSettings()
    {
        StringBuilder keyList = new();
        foreach (var kvp in _settings)
        {
            if (keyList.Length > 0) keyList.Append(',');
            keyList.Append(kvp.Key);
            PlayerPrefs.SetString(kvp.Key, kvp.Value);
        }

        PlayerPrefs.SetString(SettingsKey, keyList.ToString());
        PlayerPrefs.Save();

        EventBus.Instance?.Raise(new SettingsSavedEvent());
    }
}
