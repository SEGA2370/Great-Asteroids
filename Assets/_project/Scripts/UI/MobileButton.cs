﻿using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
public class MobileButton : MonoBehaviour
{
    public bool IsPressed { get; private set; }
    private RectTransform _buttonRect;
    private TouchControl _cachedTouch;
    private int _touchId = -1;
    void Awake()
    {
        _buttonRect = GetComponent<RectTransform>();
    }
    private void Update()
    {
        if (CheckForTouchBegan()) return;
        if (_cachedTouch == null || IsTouchEndedOrCanceled())
        {
            ResetTouch();
        }
    }
    private bool CheckForTouchBegan()
    {
        if (_touchId != -1 || Touchscreen.current == null) return false;
        foreach (var touch in Touchscreen.current.touches.Where(t => t.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Began))
        {
            var pointerPos = touch.position.ReadValue();
            if (RectTransformUtility.RectangleContainsScreenPoint(_buttonRect, pointerPos, null))
            {
                _touchId = touch.touchId.ReadValue();
                _cachedTouch = touch;
                IsPressed = true;
                return true;
            }
        }
        return false;
    }
    private bool IsTouchEndedOrCanceled()
    {
        return _cachedTouch.phase.ReadValue() is UnityEngine.InputSystem.TouchPhase.Ended or UnityEngine.InputSystem.TouchPhase.Canceled;
    }
    private void ResetTouch()
    {
        IsPressed = false;
        _touchId = -1;
        _cachedTouch = null;
    }
}