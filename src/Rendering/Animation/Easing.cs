namespace ChangeTrace.Rendering.Animation;

/// <summary>
/// Common easing functions for tweens (0 → 1).
/// </summary>
internal static class Easing
{
    /// <summary>Linear progression.</summary>
    internal static float Linear(float t) => t;

    /// <summary>Quadratic easing in (accelerating from zero velocity).</summary>
    internal static float EaseInQuad(float t) => t * t;

    /// <summary>Quadratic easing out (decelerating to zero velocity).</summary>
    internal static float EaseOutQuad(float t) => t * (2f - t);

    /// <summary>Quadratic easing in/out (acceleration until halfway, then deceleration).</summary>
    internal static float EaseInOutQuad(float t) =>
        t < 0.5f ? 2f * t * t : -1f + (4f - 2f * t) * t;

    /// <summary>Cubic easing in (accelerating from zero velocity).</summary>
    internal static float EaseInCubic(float t) => t * t * t;

    /// <summary>Cubic easing out (decelerating to zero velocity).</summary>
    internal static float EaseOutCubic(float t) => (t - 1f) * (t - 1f) * (t - 1f) + 1f;

    /// <summary>Cubic easing in/out (acceleration until halfway, then deceleration).</summary>
    internal static float EaseInOutCubic(float t) =>
        t < 0.5f ? 4f * t * t * t : (t - 1f) * (2f * t - 2f) * (2f * t - 2f) + 1f;

    /// <summary>Elastic easing out (overshooting spring-like effect).</summary>
    internal static float EaseOutElastic(float t)
    {
        if (t is 0 or 1) return t;
        const float p = 0.3f;
        return MathF.Pow(2f, -10f * t) * MathF.Sin((t - p / 4f) * (2f * MathF.PI) / p) + 1f;
    }

    /// <summary>Bouncing easing out.</summary>
    internal static float EaseOutBounce(float t)
    {
        const float n1 = 7.5625f, d1 = 2.75f;
        return t switch
        {
            < 1f / d1 => n1 * t * t,
            < 2f / d1 => n1 * (t -= 1.5f / d1) * t + 0.75f,
            < 2.5f / d1 => n1 * (t -= 2.25f / d1) * t + 0.9375f,
            _ => n1 * (t -= 2.625f / d1) * t + 0.984375f
        };
    }

    /// <summary>Sine easing in/out (smooth sinusoidal acceleration and deceleration).</summary>
    internal static float EaseInOutSine(float t) =>
        -(MathF.Cos(MathF.PI * t) - 1f) / 2f;
}