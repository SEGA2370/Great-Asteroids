using System;
using UnityEngine;
using UnityEngine.UI;
public class UIButton : MonoBehaviour
{
    private Button _button;
    private Action _onButtonClickedAction;
    public void Init(Action onButtonClickedAction)
    {
        _onButtonClickedAction = onButtonClickedAction;
    }
    private void Awake()
    {
        _button = GetComponent<Button>();
        if (_button == null)
        {
            return;
        }
    }
    private void OnEnable()
    {
        _button?.onClick.AddListener(OnButtonClicked);
    }
    private void OnDisable()
    {
        _button?.onClick.RemoveListener(OnButtonClicked);
    }
    private void OnButtonClicked()
    {
        _onButtonClickedAction?.Invoke();
    }
    public void Cleanup()
    {
        // Remove all listeners to avoid memory leaks
        _button?.onClick.RemoveAllListeners();
        _onButtonClickedAction = null;
    }
}