using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public abstract class PlayerInputBase : MonoBehaviour
{
    public abstract float GetRotationInput();
    public abstract bool GetThrustInput();
    public abstract bool GetFireInput();
    public abstract bool AnyInputThisFrame { get; }
}