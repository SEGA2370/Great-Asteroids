using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
#if !UNITY_IOS && !UNITY_ANDROID
using UnityEngine.InputSystem;
#endif

public class StartMenuUI : MonoBehaviour
{
    [Header("Choose Mode")]
    [SerializeField] private Button _level1Button;
    [SerializeField] private Button _level2Button;
    [SerializeField] private Button _level3Button;
    [SerializeField] private Button _level4Button;

    [Header("UI Elements")]
    [SerializeField] private Image _fadePanel;
    [SerializeField] private GameObject _controlsPanel;
    [SerializeField] private GameObject _pressSpaceText;
    [SerializeField] private Button _playButton;
    [SerializeField] private TMP_Text _rotateControlsText;
    [SerializeField] private TMP_Text _thrustFireControlsText;
    [SerializeField] private TMP_Text _hyperspaceControlText;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private UIButton _settingsButton;

    private PlayerInputBase _playerInput;

#if !UNITY_IOS && !UNITY_ANDROID
    private PlayerKeyboardInput _playerKeyboardInput;
#endif

    void Awake()
    {
#if UNITY_IOS || UNITY_ANDROID
        _playButton.gameObject.SetActive(true);
        _pressSpaceText.SetActive(false);
#else
        _playerInput = gameObject.AddComponent<PlayerKeyboardInput>();
        _playerKeyboardInput = (PlayerKeyboardInput)_playerInput;
        _playButton.gameObject.SetActive(false);
        _pressSpaceText.SetActive(true);
#endif
    }

    void OnEnable()
    {
        _audioSource.volume = 1f;
        _fadePanel.DOFade(0f, 1f);
        _settingsButton?.Init(LoadSettingsScene);

        _level1Button.onClick.AddListener(() => LoadScene("Game"));
        _level2Button.onClick.AddListener(() => LoadScene("Game LVL_2"));
        _level3Button.onClick.AddListener(() => LoadScene("Game LVL_3"));
        _level4Button.onClick.AddListener(() => LoadScene("Game LVL_4"));

        _playButton.onClick.AddListener(() => LoadScene("Game"));

#if !UNITY_IOS && !UNITY_ANDROID
        EventBus.Instance.Subscribe<SettingsSavedEvent>(OnSettingsUpdated);
#endif

        UpdateControlsText();
    }

    void OnDisable()
    {
        _level1Button.onClick.RemoveAllListeners();
        _level2Button.onClick.RemoveAllListeners();
        _level3Button.onClick.RemoveAllListeners();
        _level4Button.onClick.RemoveAllListeners();
        _playButton.onClick.RemoveAllListeners();

#if !UNITY_IOS && !UNITY_ANDROID
        EventBus.Instance?.Unsubscribe<SettingsSavedEvent>(OnSettingsUpdated);
#endif
    }

#if !UNITY_IOS && !UNITY_ANDROID
    void Update()
    {
        if (Keyboard.current?.spaceKey.wasPressedThisFrame == true)
        {
            LoadScene("Game");
        }
    }
#endif

    private void LoadScene(string sceneName)
    {
        _audioSource.DOFade(0f, 2f);
        _fadePanel.DOFade(1f, 1f).OnComplete(() =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }

    private void LoadSettingsScene()
    {
        SceneManager.LoadScene("Settings", LoadSceneMode.Additive);
    }

#if !UNITY_IOS && !UNITY_ANDROID
    private void OnSettingsUpdated(SettingsSavedEvent _)
    {
        _playerKeyboardInput.LoadKeyBindings();
        UpdateControlsText();
    }
#endif

    private void UpdateControlsText()
    {
#if !UNITY_IOS && !UNITY_ANDROID
        _controlsPanel.SetActive(true);
        _rotateControlsText.text = $"{_playerKeyboardInput.RotateLeftKey} / {_playerKeyboardInput.RotateRightKey} - Rotate Left / Right";
        _thrustFireControlsText.text = $"{_playerKeyboardInput.ThrustKey} / {_playerKeyboardInput.FireKey} - Thrust / Fire";
        _hyperspaceControlText.text = $"{_playerKeyboardInput.HyperspaceKey} - Hyperspace";
#else
        _controlsPanel.gameObject.SetActive(false);
#endif
    }
}
