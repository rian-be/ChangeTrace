namespace ChangeTrace.Rendering.Animation;

/// <summary>
/// Represents single particle in animation/particle.
/// Tracks position, velocity, lifetime, color, size, and transparency.
/// </summary>
internal sealed class Particle
{
    /// <summary>Current position of the particle.</summary>
    internal Vec2 Position { get; set; }

    /// <summary>Current velocity of the particle.</summary>
    internal Vec2 Velocity { get; set; }

    /// <summary>Current alpha (opacity) of the particle. 1 = fully visible, 0 = invisible.</summary>
    internal float Alpha { get; set; } = 1f;

    /// <summary>Radius/size of the particle.</summary>
    internal float Size { get; set; } = 3f;

    /// <summary>Color of the particle, stored as packed RGB.</summary>
    internal uint Color { get; }

    /// <summary>Total lifetime of the particle in seconds.</summary>
    internal float Lifetime { get; }

    private float _elapsed;

    /// <summary>Indicates whether the particle has exceeded its lifetime.</summary>
    internal bool IsDead => _elapsed >= Lifetime;
    
    /// <param name="position">Starting position.</param>
    /// <param name="velocity">Initial velocity.</param>
    /// <param name="lifetime">Duration the particle lives in seconds.</param>
    /// <param name="color">Particle color (packed RGB).</param>
    /// <param name="size">Radius/size of the particle.</param>
    internal Particle(Vec2 position, Vec2 velocity, float lifetime, uint color, float size)
    {
        Position = position;
        Velocity = velocity;
        Lifetime = lifetime;
        Color = color;
        Size = size;
    }

    /// <summary>
    /// Advances particle simulation by time step.
    /// Updates position, applies friction to velocity, and reduces alpha over lifetime.
    /// </summary>
    /// <param name="dt">Time step in seconds.</param>
    internal void Tick(float dt)
    {
        _elapsed += dt;
        Position += Velocity * dt;
        Velocity *= 0.97f; // apply simple friction
        Alpha = Math.Max(0f, 1f - _elapsed / Lifetime);
    }
}