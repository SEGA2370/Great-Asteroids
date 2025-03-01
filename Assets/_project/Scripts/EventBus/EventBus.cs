using System;
using System.Collections.Generic;
using UnityEngine;
public class EventBus : SingletonMonoBehaviour<EventBus>
{
    private readonly Dictionary<Type, Delegate> _eventDictionary = new();
    private readonly object _lock = new(); // Lock object for thread safety
    /// <summary>
    /// Subscribes a listener to the specified event type.
    /// </summary>
    public void Subscribe<T>(Action<T> listener)
    {
        if (listener == null) throw new ArgumentNullException(nameof(listener));
        var eventType = typeof(T);
        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out var currentDelegate))
            {
                _eventDictionary[eventType] = listener;
            }
            else
            {
                _eventDictionary[eventType] = Delegate.Combine(currentDelegate, listener);
            }
        }
    }
    /// <summary>
    /// Unsubscribes a listener from the specified event type.
    /// </summary>
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
                {
                    _eventDictionary.Remove(eventType);
                }
                else
                {
                    _eventDictionary[eventType] = currentDelegate;
                }
            }
        }
    }
    /// <summary>
    /// Raises an event of the specified type with the given arguments.
    /// </summary>
    public void Raise<T>(T eventArgs)
    {
        var eventType = typeof(T);
        Delegate currentDelegate;
        lock (_lock)
        {
            if (!_eventDictionary.TryGetValue(eventType, out currentDelegate) || currentDelegate == null) return;
        }
        if (currentDelegate is Action<T> action)
        {
            foreach (var listener in action.GetInvocationList())
            {
                try
                {
                    ((Action<T>)listener)?.Invoke(eventArgs);
                }
                catch (Exception)
                {
                   return;
                }
            }
        }
        else
        {
            return;
        }
    }
    /// <summary>
    /// Checks if there are any listeners subscribed to the specified event type.
    /// </summary>
    public bool HasListeners<T>()
    {
        var eventType = typeof(T);
        lock (_lock)
        {
            return _eventDictionary.TryGetValue(eventType, out var currentDelegate) && currentDelegate != null;
        }
    }
    /// <summary>
    /// Clears all event subscriptions. Use cautiously.
    /// </summary>
    public void ClearAll()
    {
        lock (_lock)
        {
            _eventDictionary.Clear();
        }
    }
    /// <summary>
    /// Clears all listeners for a specific event type.
    /// </summary>
    public void ClearEvent<T>()
    {
        var eventType = typeof(T);
        lock (_lock)
        {
            if (_eventDictionary.ContainsKey(eventType))
            {
                _eventDictionary.Remove(eventType);
            }
        }
    }
}