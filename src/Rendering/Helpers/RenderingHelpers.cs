using System.Numerics;

namespace ChangeTrace.Rendering.Helpers;

/// <summary>
/// Utility helpers for generating scene positions and colors.
/// </summary>
internal static class RenderingHelpers
{
    private static readonly Random Rng =
        Random.Shared;

    /// <summary>
    /// Generates a random position near the scene origin.
    /// </summary>
    public static Vec2 RandomNear()
    {
        float angle =
            Rng.NextSingle() * MathF.PI * 2f;

        float dist =
            5f + Rng.NextSingle() * 15f;

        return new Vec2(
            MathF.Cos(angle) * dist,
            MathF.Sin(angle) * dist);
    }

    /// <summary>
    /// Generates random position along scene bounds.
    /// </summary>
    public static Vec2 RandomEdge()
    {
        float side =
            Rng.NextSingle() * 4f;

        float t =
            Rng.NextSingle();

        const float r = 200f;

        return (int)side switch
        {
            0 => new Vec2(-r, -r + t * 2 * r),
            1 => new Vec2( r, -r + t * 2 * r),
            2 => new Vec2(-r + t * 2 * r, -r),
            _ => new Vec2(-r + t * 2 * r,  r)
        };
    }

    /// <summary>
    /// Converts packed RGB integer into a normalized color vector.
    /// </summary>
    public static Vector4 ColorFromUInt(uint rgb)
    {
        float r =
            ((rgb >> 16) & 0xFF) / 255f;

        float g =
            ((rgb >> 8) & 0xFF) / 255f;

        float b =
            (rgb & 0xFF) / 255f;

        return new Vector4(r, g, b, 1f);
    }
}