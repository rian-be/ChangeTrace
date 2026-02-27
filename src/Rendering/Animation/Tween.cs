namespace ChangeTrace.Rendering.Animation;

/// <summary>
/// Represents single value interpolation (tween) over time with easing.
/// </summary>
/// <typeparam name="T">Type of value being tweened (e.g., float, Vec2).</typeparam>
internal sealed class Tween<T>
{
    private readonly T _from;
    private readonly T _to;
    private readonly float  _durationSec;
    private readonly EasingFn  _easing;
    private readonly Func<T, T, float, T> _lerp;
    private readonly Action<T>  _onUpdate;
    private readonly Action? _onComplete;

    private float _elapsed;
    internal bool IsComplete { get; private set; }

    internal Tween(
        T from,
        T to,
        float   durationSec,
        EasingFn easing,
        Func<T, T, float, T> lerp,
        Action<T> onUpdate,
        Action?   onComplete = null)
    {
        _from = from;
        _to = to;
        _durationSec = durationSec;
        _easing = easing;
        _lerp = lerp;
        _onUpdate = onUpdate;
        _onComplete  = onComplete;
    }

    /// <summary>
    /// Advances tween by given delta time.
    /// </summary>
    /// <param name="deltaSeconds">Time elapsed since last tick in seconds.</param>
    internal void Tick(float deltaSeconds)
    {
        if (IsComplete) return;
        _elapsed += deltaSeconds;

        var t   = Math.Clamp(_elapsed / _durationSec, 0f, 1f);
        var et  = _easing(t);
        _onUpdate(_lerp(_from, _to, et));

        if (!(_elapsed >= _durationSec)) return;
        IsComplete = true;
        _onComplete?.Invoke();
    }
}