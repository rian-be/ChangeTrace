namespace ChangeTrace.Rendering;

/// <summary>
/// Represents a 2D vector with single-precision floating point components.
/// Provides common vector operations used for positions, directions, and interpolation in rendering.
/// </summary>
internal readonly record struct Vec2(float X, float Y)
{
    /// <summary>Vector with both components zero.</summary>
    internal static readonly Vec2 Zero = new(0, 0);

    /// <summary>Vector with both components equal to one.</summary>
    internal static readonly Vec2 One = new(1, 1);

    /// <summary>Squared length of the vector (avoids square root).</summary>
    internal float LengthSq => X * X + Y * Y;

    /// <summary>Euclidean length (magnitude) of the vector.</summary>
    internal float Length => MathF.Sqrt(LengthSq);

    /// <summary>Returns a normalized (unit-length) vector in the same direction.
    /// Returns <see cref="Zero"/> if vector length is near zero.</summary>
    internal Vec2 Normalized()
    {
        var len = Length;
        return len < 1e-9f ? Zero : new Vec2(X / len, Y / len);
    }

    /// <summary>Linearly interpolates between this vector and <paramref name="to"/> by fraction <paramref name="t"/>.</summary>
    /// <param name="to">Target vector.</param>
    /// <param name="t">Interpolation factor (0 = this vector, 1 = target vector).</param>
    /// <returns>Interpolated vector.</returns>
    internal Vec2 Lerp(Vec2 to, float t) =>
        new(X + (to.X - X) * t, Y + (to.Y - Y) * t);

    /// <summary>Computes the dot product with another vector.</summary>
    /// <param name="other">Other vector.</param>
    /// <returns>Dot product (scalar).</returns>
    internal float Dot(Vec2 other) => X * other.X + Y * other.Y;

    /// <summary>Vector addition.</summary>
    public static Vec2 operator +(Vec2 a, Vec2 b) => new(a.X + b.X, a.Y + b.Y);

    /// <summary>Vector subtraction.</summary>
    public static Vec2 operator -(Vec2 a, Vec2 b) => new(a.X - b.X, a.Y - b.Y);

    /// <summary>Scale vector by scalar.</summary>
    public static Vec2 operator *(Vec2 v, float s) => new(v.X * s, v.Y * s);

    /// <summary>Scale vector by scalar (commutative).</summary>
    public static Vec2 operator *(float s, Vec2 v) => v * s;

    /// <summary>Divide vector by scalar.</summary>
    public static Vec2 operator /(Vec2 v, float s) => new(v.X / s, v.Y / s);

    /// <summary>Returns a human-readable string representation of the vector.</summary>
    public override string ToString() => $"({X:F1}, {Y:F1})";
}