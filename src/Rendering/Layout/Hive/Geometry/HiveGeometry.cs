using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Geometry;

/// <summary>
/// Shared geometry and deterministic hashing helpers used by hive layouts.
/// </summary>
internal static class HiveGeometry
{
    /// <summary>
    /// Creates a normalized direction vector from an angle in radians.
    /// </summary>
    public static Vec2 Direction(float angle) =>
        new(
            MathF.Cos(angle),
            MathF.Sin(angle));

    /// <summary>
    /// Produces a deterministic angle for a string identifier.
    /// </summary>
    public static float StableAngle(string id) => 
        Hash01(id, 777) * MathF.Tau;

    /// <summary>
    /// Produces a stable pseudo-random value in the range [0, 1].
    /// </summary>
    public static float Hash01(string text, int seed)
    {
        unchecked
        {
            var hash = seed;

            foreach (var c in text)
                hash = hash * 31 + c;

            hash ^= hash >> 16;
            hash *= 0x7feb352d;
            hash ^= hash >> 15;
            hash *= unchecked((int)0x846ca68b);
            hash ^= hash >> 16;

            return (hash & 0x00FFFFFF) / (float)0x01000000;
        }
    }

    /// <summary>
    /// Assigns a target position and clears transient physics state.
    /// </summary>
    public static void SetTarget(
        SceneNode node,
        Vec2 target)
    {
        node.HomePosition = target;
        node.Velocity = Vec2.Zero;
        node.Force = Vec2.Zero;
    }
}