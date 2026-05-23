using System.Numerics;

namespace ChangeTrace.Rendering.Colors;

/// <summary>
/// Provides deterministic color mapping for file extensions.
/// </summary>
/// <remarks>
/// Uses extension registry lookup with fallback color for unknown file types.
/// </remarks>
internal static class ExtensionColorPalette
{
    /// <summary>
    /// Gets display color for specified file paths.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>RGBA color vector assigned to file extension.</returns>
    public static Vector4 ForPath(string path)
    {
        var ext = Path.GetExtension(path);

        return ColorPalette.UIntToVec4(
            ExtensionColorRegistry.Colors.TryGetValue(ext, out var c)
                ? c
                : 0x888888u);
    }
}