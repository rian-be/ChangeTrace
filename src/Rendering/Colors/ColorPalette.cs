using System.Numerics;

namespace ChangeTrace.Rendering.Colors;

/// <summary>
/// Central color utility and palette access layer used by rendering systems.
/// </summary>
/// <remarks>
/// Provides actor and file-path color mapping as well as color conversion helpers.
/// </remarks>
internal static class ColorPalette
{
    /// <summary>
    /// Gets deterministic color assigned to actor identifier.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>RGBA color vector assigned to an actor.</returns>
    public static Vector4 ForActor(string actor) =>
        ActorColorPalette.ForActor(actor);

    /// <summary>
    /// Gets deterministic color assigned to file paths.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>RGBA color vector assigned to file paths.</returns>
    public static Vector4 ForFilePath(string path) =>
        ExtensionColorPalette.ForPath(path);

    /// <summary>
    /// Converts packed RGB integer into a normalized RGBA vector.
    /// </summary>
    /// <param name="rgb">Packed RGB color value.</param>
    /// <param name="alpha">Alpha component.</param>
    /// <returns>Normalized RGBA vector.</returns>
    public static Vector4 UIntToVec4(uint rgb, float alpha = 1f)
    {
        float r = ((rgb >> 16) & 0xFF) / 255f;
        float g = ((rgb >> 8) & 0xFF) / 255f;
        float b = (rgb & 0xFF) / 255f;

        return new Vector4(r, g, b, alpha);
    }

    /// <summary>
    /// Converts a normalized RGBA vector into a packed RGB integer.
    /// </summary>
    /// <param name="c">Normalized RGBA vector.</param>
    /// <returns>Packed RGB color value.</returns>
    internal static uint Vec4ToUInt(Vector4 c)
    {
        uint r = (uint)(Math.Clamp(c.X, 0f, 1f) * 255f);
        uint g = (uint)(Math.Clamp(c.Y, 0f, 1f) * 255f);
        uint b = (uint)(Math.Clamp(c.Z, 0f, 1f) * 255f);

        return (r << 16) | (g << 8) | b;
    }
}