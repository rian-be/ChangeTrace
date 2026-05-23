using System.Numerics;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
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
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class AnimationSystem : IAnimationSystem
{
    private readonly List<Tween<Vec2>> _vecTweens = [];
    private readonly List<Tween<float>> _floatTweens = [];
    private readonly List<Particle> _particles = [];

    private readonly Lock _vecLock = new();
    private readonly Lock _floatLock = new();
    private readonly Lock _particleLock = new();

    private const int MaxParticles = 15000;
    
    public int ParticleCount
    {
        get
        {
            lock (_particleLock) return _particles.Count;
        }
    }
    
    /// <summary>
    /// Creates <see cref="Vec2"/> tween.
    /// </summary>
    /// <param name="from">Starting vector.</param>
    /// <param name="to">Target vector.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="easing">Easing function.</param>
    /// <param name="onUpdate">Callback invoked on each update.</param>
    /// <param name="onComplete">Optional callback invoked on completion.</param>
    /// <param name="tag">Optional tag for identification.</param>
    public void TweenVec2(
        Vec2 from, Vec2 to, float duration, EasingFn easing,
        Action<Vec2> onUpdate, Action? onComplete = null, string? tag = null)
    {
        lock (_vecLock)
        {
            AddTween(_vecTweens, from, to, duration, easing, (a, b, t) => a.Lerp(b, t), onUpdate, onComplete, tag);
        }
    }

    /// <summary>
    /// Creates <see cref="float"/> tween.
    /// </summary>
    /// <param name="from">Starting value.</param>
    /// <param name="to">Target value.</param>
    /// <param name="duration">Duration in seconds.</param>
    /// <param name="easing">Easing function.</param>
    /// <param name="onUpdate">Callback invoked on each update.</param>
    /// <param name="onComplete">Optional callback invoked on completion.</param>
    /// <param name="tag">Optional tag for identification.</param>
    public void TweenFloat(
        float from, float to, float duration, EasingFn easing,
        Action<float> onUpdate, Action? onComplete = null, string? tag = null)
    {
        lock (_floatLock)
        {
            AddTween(_floatTweens, from, to, duration, easing, (a, b, t) => a + (b - a) * t, onUpdate, onComplete, tag);
        }
    }
    
    /// <summary>
    /// Creates particle burst at a given origin.
    /// </summary>
    /// <param name="origin">Origin of the burst.</param>
    /// <param name="count">Number of particles.</param>
    /// <param name="color">Particle color (packed RGB).</param>
    /// <param name="speed">Maximum particle speed.</param>
    /// <param name="lifetime">Particle lifetime in seconds.</param>
    public void Burst(Vec2 origin, int count, Vector4 color, float speed = 60f, float lifetime = 1.2f)
    {
        var rng = Random.Shared;
        lock (_particleLock)
        {
            if (_particles.Count >= MaxParticles) return;
            
            int actualCount = Math.Min(count, MaxParticles - _particles.Count);
            for (int i = 0; i < actualCount; i++)
            {
                var particle = CreateParticle(origin, color, speed, lifetime, rng);
                _particles.Add(particle);
            }
        }

        static Particle CreateParticle(Vec2 origin, Vector4 color, float speed, float lifetime, Random rng)
        {
            float angle = rng.NextSingle() * MathF.PI * 2f;
            float spd = speed * (0.4f + rng.NextSingle() * 0.6f);
            var vel = new Vec2(MathF.Cos(angle) * spd, MathF.Sin(angle) * spd);
            float life  = lifetime * (0.7f + rng.NextSingle() * 0.3f);
            float size  = 2f + rng.NextSingle() * 3f;
            return new Particle(origin, vel, life, color, size);
        }
    }

    /// <summary>
    /// Ticks all active tweens and particles, removing completed ones in a single pass.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time in seconds since the last tick.</param>
    public void Tick(float deltaSeconds)
    {
        lock (_vecLock)
        {
            _vecTweens.RemoveAll(t => {
                t.Tick(deltaSeconds);
                return t.IsComplete;
            });
        }

        lock (_floatLock)
        {
            _floatTweens.RemoveAll(t => {
                t.Tick(deltaSeconds);
                return t.IsComplete;
            });
        }

        lock (_particleLock)
        {
            _particles.RemoveAll(p => {
                p.Tick(deltaSeconds);
                return p.IsDead;
            });
        }
    }

    /// <summary>
    /// Snapshots all active particles into the provided list without allocation.
    /// </summary>
    public void SnapshotParticles(List<ParticleSnapshot> target)
    {
        lock (_particleLock)
        {
            foreach (var p in _particles)
            {
                target.Add(new ParticleSnapshot(p.Position, p.Alpha, p.Size, p.Color));
            }
        }
    }
    
    /// <summary>
    /// Clears all tweens and particles.
    /// </summary>
    public void Clear()
    {
        lock (_vecLock) _vecTweens.Clear();
        lock (_floatLock) _floatTweens.Clear();
        lock (_particleLock) _particles.Clear();
    }

    private static void AddTween<T>(
        List<Tween<T>> list,
        T from, T to, float duration, EasingFn easing,
        Func<T, T, float, T> lerp,
        Action<T> onUpdate, Action? onComplete,
        string? tag
     )
    {
        if (tag != null)
        {
            list.RemoveAll(t => t.Tag == tag);
        }
        list.Add(new Tween<T>(from, to, duration, easing, lerp, onUpdate, onComplete, tag));
    }
}