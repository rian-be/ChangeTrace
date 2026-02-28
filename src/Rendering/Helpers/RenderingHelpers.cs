namespace ChangeTrace.Rendering.Helpers;

/// <summary>
/// Utility methods for generating positions in the scene for rendering purposes.
/// </summary>
internal static class RenderingHelpers
{
    private static readonly Random Rng = Random.Shared;

    /// <summary>
    /// Generates random position near the origin (for spawning nodes randomly around center).
    /// </summary>
    public static Vec2 RandomNear()
    {
        var angle = Rng.NextSingle() * MathF.PI * 2f;
        var dist  = 50f + Rng.NextSingle() * 200f;
        return new Vec2(MathF.Cos(angle) * dist, MathF.Sin(angle) * dist);
    }

    /// <summary>
    /// Generates random position along edges of scene (offscreen).
    /// </summary>
    public static Vec2 RandomEdge()
    {
        var side = Rng.NextSingle() * 4f;
        var t = Rng.NextSingle();
        const float r = 700f;

        return (int)side switch
        {
            0 => new Vec2(-r, -r + t * 2 * r),
            1 => new Vec2( r, -r + t * 2 * r),
            2 => new Vec2(-r + t * 2 * r, -r),
            _ => new Vec2(-r + t * 2 * r,  r)
        };
    }
}