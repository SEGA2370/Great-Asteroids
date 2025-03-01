using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
public class TimerManager : SingletonMonoBehaviour<TimerManager>
{
    private readonly Dictionary<Type, ObjectPool<Timer>> _pools = new();
    private readonly List<Timer> _timers = new();
    /// <summary>
    /// Creates a timer of the specified type and sets its initial value.
    /// </summary>
    /// <typeparam name="T">The type of timer to create.</typeparam>
    /// <param name="value">The initial time value for the timer.</param>
    /// <returns>The created timer instance.</returns>
    public Timer CreateTimer<T>(float value = 0f) where T : Timer, new()
    {
        var pool = GetTimerPool<T>();
        var timer = pool.Get();
        timer.SetInitialTime(value);
        _timers.Add(timer);
        return timer;
    }
    /// <summary>
    /// Releases a timer back to its pool and stops it if active.
    /// </summary>
    /// <typeparam name="T">The type of timer to release.</typeparam>
    /// <param name="timer">The timer instance to release.</param>
    public void ReleaseTimer<T>(Timer timer) where T : Timer, new()
    {
        if (timer == null || TimerManager.Instance == null) return;
        timer.Stop();
        timer.OnTimerStop = null; // Remove any lingering callbacks
        if (_timers.Contains(timer))
            _timers.Remove(timer);
        var pool = GetTimerPool<T>();
        pool.Release(timer);
    }
    /// <summary>
    /// Retrieves the object pool for a specific timer type, creating it if necessary.
    /// </summary>
    /// <typeparam name="T">The type of timer.</typeparam>
    /// <returns>The object pool for the specified timer type.</returns>
    private IObjectPool<Timer> GetTimerPool<T>() where T : Timer, new()
    {
        var type = typeof(T);
        if (!_pools.ContainsKey(type))
        {
            _pools[type] = new ObjectPool<Timer>(() => new T());
        }
        return _pools[type];
    }
    /// <summary>
    /// Updates all active timers.
    /// </summary>
    public void ClearAllTimers()
    {
        foreach (var timer in _timers)
        {
            timer?.Stop();
        }
        _timers.Clear();
    }
    private void Update()
    {
        for (int i = _timers.Count - 1; i >= 0; i--)
        {
            var timer = _timers[i];
            if (timer == null)
            {
                _timers.RemoveAt(i);
                continue;
            }
            timer.Tick(Time.deltaTime);
        }
    }
}