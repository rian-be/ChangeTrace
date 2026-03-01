using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Rendering.Animation;

/// <summary>
/// Animation and particle system for renderer.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Supports <see cref="Vec2"/> and <see cref="float"/> tweens with arbitrary easing functions.</item>
/// <item>Supports particle bursts with random spread, speed, size, and lifetime.</item>
/// <item>Ticks all active tweens and particles, removing completed ones.</item>
/// <item>Exposes <see cref="Particles"/> for read-only access to all active particles.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class AnimationSystem : IAnimationSystem
{
    private readonly List<Tween<Vec2>> _vecTweens   = [];
    private readonly List<Tween<float>> _floatTweens = [];
    private readonly List<Particle> _particles   = [];

    /// <summary>
    /// Creates <see cref="Vec2"/> tween.
    /// </summary>
    /// <param name="from">Starting vector.</param>
    /// <param name="to">Target vector.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="easing">Easing function.</param>
    /// <param name="onUpdate">Callback invoked on each update.</param>
    /// <param name="onComplete">Optional callback invoked on completion.</param>
    public void TweenVec2(
        Vec2 from, Vec2 to, float duration, EasingFn easing,
        Action<Vec2> onUpdate, Action? onComplete = null)
        => AddTween(_vecTweens, from, to, duration, easing, (a, b, t) => a.Lerp(b, t), onUpdate, onComplete);

    /// <summary>
    /// Creates <see cref="float"/> tween.
    /// </summary>
    /// <param name="from">Starting value.</param>
    /// <param name="to">Target value.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="easing">Easing function.</param>
    /// <param name="onUpdate">Callback invoked on each update.</param>
    /// <param name="onComplete">Optional callback invoked on completion.</param>
    public void TweenFloat(
        float from, float to, float duration, EasingFn easing,
        Action<float> onUpdate, Action? onComplete = null)
        => AddTween(_floatTweens, from, to, duration, easing, (a, b, t) => a + (b - a) * t, onUpdate, onComplete);

    /// <summary>
    /// Creates particle burst at given origin.
    /// </summary>
    /// <param name="origin">Origin of the burst.</param>
    /// <param name="count">Number of particles.</param>
    /// <param name="color">Particle color (packed RGB).</param>
    /// <param name="speed">Maximum particle speed.</param>
    /// <param name="lifetime">Particle lifetime in seconds.</param>
    public void Burst(Vec2 origin, int count, uint color, float speed = 60f, float lifetime = 1.2f)
    {
        var rng = Random.Shared;
        for (int i = 0; i < count; i++)
        {
            var particle = CreateParticle(origin, color, speed, lifetime, rng);
            _particles.Add(particle);
        }

        static Particle CreateParticle(Vec2 origin, uint color, float speed, float lifetime, Random rng)
        {
            float angle = rng.NextSingle() * MathF.PI * 2f;
            float spd   = speed * (0.4f + rng.NextSingle() * 0.6f);
            var vel     = new Vec2(MathF.Cos(angle) * spd, MathF.Sin(angle) * spd);
            float life  = lifetime * (0.7f + rng.NextSingle() * 0.3f);
            float size  = 2f + rng.NextSingle() * 3f;
            return new Particle(origin, vel, life, color, size);
        }
    }

    /// <summary>
    /// Ticks all active tweens and particles, removing completed ones.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time in seconds since last tick.</param>
    public void Tick(float deltaSeconds)
    {
        TickAndRemoveCompleted(_vecTweens, t => { t.Tick(deltaSeconds); return t.IsComplete; });
        TickAndRemoveCompleted(_floatTweens, t => { t.Tick(deltaSeconds); return t.IsComplete; });
        TickAndRemoveCompleted(_particles, p => { p.Tick(deltaSeconds); return p.IsDead; });
    }

    /// <summary>
    /// Read-only list of all active particles.
    /// </summary>
    public IReadOnlyList<Particle> Particles => _particles;

    /// <summary>
    /// Clears all tweens and particles.
    /// </summary>
    public void Clear()
    {
        _vecTweens.Clear();
        _floatTweens.Clear();
        _particles.Clear();
    }

    private static void AddTween<T>(
        List<Tween<T>> list,
        T from, T to, float duration, EasingFn easing,
        Func<T, T, float, T> lerp,
        Action<T> onUpdate, Action? onComplete
     ) => list.Add(new Tween<T>(from, to, duration, easing, lerp, onUpdate, onComplete));

    private static void TickAndRemoveCompleted<T>(
        List<T> list,
        Func<T, bool> isDone)
    {
        for (var i = list.Count - 1; i >= 0; i--)
        {
            if (isDone(list[i]))
                list.RemoveAt(i);
        }
    }
}