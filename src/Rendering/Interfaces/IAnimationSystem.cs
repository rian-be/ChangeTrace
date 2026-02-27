using ChangeTrace.Rendering.Animation;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Defines an animation and particle system for the renderer.
/// Supports particle bursts, tweening, and per-frame updates.
/// </summary>
internal interface IAnimationSystem
{
    /// <summary>
    /// Gets currently active particles in the system.
    /// Immutable snapshot for rendering purposes.
    /// </summary>
    IReadOnlyList<Particle> Particles { get; }

    /// <summary>
    /// Animates <see cref="Vec2"/> value over time using specified easing function.
    /// </summary>
    /// <param name="from">Starting value.</param>
    /// <param name="to">Target value.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="easing">Easing function to apply.</param>
    /// <param name="onUpdate">Callback invoked each frame with the interpolated value.</param>
    /// <param name="onComplete">Optional callback invoked when tween completes.</param>
    void TweenVec2(Vec2 from, Vec2 to, float duration, EasingFn easing,
        Action<Vec2> onUpdate, Action? onComplete = null);

    /// <summary>
    /// Animates <see cref="float"/> value over time using a specified easing function.
    /// </summary>
    /// <param name="from">Starting value.</param>
    /// <param name="to">Target value.</param>
    /// <param name="duration">Duration of the tween in seconds.</param>
    /// <param name="easing">Easing function to apply.</param>
    /// <param name="onUpdate">Callback invoked each frame with the interpolated value.</param>
    /// <param name="onComplete">Optional callback invoked when tween completes.</param>
    void TweenFloat(float from, float to, float duration, EasingFn easing,
        Action<float> onUpdate, Action? onComplete = null);

    /// <summary>
    /// Spawns burst of particles at given origin.
    /// </summary>
    /// <param name="origin">Starting position of the burst.</param>
    /// <param name="count">Number of particles to spawn.</param>
    /// <param name="color">Color of the particles (packed RGB).</param>
    /// <param name="speed">Initial speed of the particles.</param>
    /// <param name="lifetime">Lifetime of each particle in seconds.</param>
    void Burst(Vec2 origin, int count, uint color, float speed = 60f, float lifetime = 1.2f);

    /// <summary>
    /// Advances all animations and particles by time step.
    /// Should be called once per frame.
    /// </summary>
    /// <param name="deltaSeconds">Elapsed time since last tick in seconds.</param>
    void Tick(float deltaSeconds);

    /// <summary>
    /// Clears all active particles and ongoing tweens.
    /// Resets system state for fresh start.
    /// </summary>
    void Clear();
}