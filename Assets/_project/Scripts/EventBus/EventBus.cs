using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Thread-safe, strongly typed event dispatcher using singleton pattern.
/// </summary>
public class EventBus : SingletonMonoBehaviour<EventBus>
{
    private readonly Dictionary<Type, Delegate> _eventDictionary = new();
    private readonly object _lock = new();

    /// <summary>Subscribe to an event of type T.</summary>
    public void Subscribe<T>(Action<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var eventType = typeof(T);

        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out var currentDelegate))
                _eventDictionary[eventType] = listener;
            else
                _eventDictionary[eventType] = Delegate.Combine(currentDelegate, listener);
        }
    }

    /// <summary>Unsubscribe from an event of type T.</summary>
    public void Unsubscribe<T>(Action<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var eventType = typeof(T);

        lock (_lock)
        {
            if (_eventDictionary.TryGetValue(eventType, out var currentDelegate))
            {
                currentDelegate = Delegate.Remove(currentDelegate, listener);
                if (currentDelegate == null)
                    _eventDictionary.Remove(eventType);
                else
                    _eventDictionary[eventType] = currentDelegate;
            }
        }
    }

    /// <summary>Raise an event of type T with the given arguments.</summary>
    public void Raise<T>(T eventArgs)
    {
        var eventType = typeof(T);
        Delegate currentDelegate;

        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out currentDelegate) || currentDelegate == null)
                return;
        }

        if (currentDelegate is Action<T> action)
        {
            foreach (var listener in action.GetInvocationList())
            {
                try
                {
                    ((Action<T>)listener)?.Invoke(eventArgs);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"EventBus: Exception during event dispatch: {ex.Message}");
                }
            }
        }
    }

    public bool HasListeners<T>()
    {
        var eventType = typeof(T);
        lock (_lock)
        {
            return _eventDictionary.TryGetValue(eventType, out var currentDelegate) && currentDelegate != null;
        }
    }

    public void ClearAll()
    {
        lock (_lock)
        {
            _eventDictionary.Clear();
        }
    }

    public void ClearEvent<T>()
    {
        var eventType = typeof(T);
        lock (_lock)
        {
            _eventDictionary.Remove(eventType);
        }
    }
}
