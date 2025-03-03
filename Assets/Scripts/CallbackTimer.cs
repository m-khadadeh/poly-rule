using UnityEngine;

/// <summary>
/// A small timer class that invokes a callback action when the time it sets has been passed.
/// This class opts for a polling implementation rather than a coroutine to allow it to be paused for any reason.
/// </summary>
/// <remarks>
/// <c>ProcessTimer()</c> must be called each frame the timer needs to count down time;
/// if this function is not called, the timer will not tick down.
/// </remarks>
public class CallbackTimer
{
    float _timeRemaining;
    System.Action _callback;

    /// <summary>
    /// Creates a timer that can count down from <c><paramref name="timeMilliseconds"/></c> milliseconds to zero
    /// and invokes <c><paramref name="callback"/></c> when it hits and/or goes below 0.
    /// </summary>
    /// <param name="timeMilliseconds">The length of the time to count down for in milliseconds.</param>
    /// <param name="callback">The action to invoke when the timer hits and/or goes below 0.</param>
    /// <remarks>
    /// This class will not automatically process time. Call <c>ProcessTimer()</c> each frame you want time processed
    /// in order to count down.
    /// </remarks>
    public CallbackTimer(int timeMilliseconds, System.Action callback)
    {
        _timeRemaining = (float)timeMilliseconds / 1000.0f;
        _callback = callback;
    }

    /// <summary>
    /// Counts down the time remaining on the timer.
    /// If the time hits 0, it invokes the callback action.
    /// </summary>
    /// <remarks>
    /// The callback action will be invoked each time this function called after the timer has finished counting down.
    /// If you only want to invoke the callback action once, stop calling this function after the callback is invoked.
    /// </remarks>
    public void ProcessTimer()
    {
        if(_timeRemaining > 0)
        {
            _timeRemaining -= Time.deltaTime;
        }
        else
        {
            _callback.Invoke();
        }
    }
}
