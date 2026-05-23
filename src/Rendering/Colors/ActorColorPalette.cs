using System.Numerics;

namespace ChangeTrace.Rendering.Colors;

/// <summary>
/// Provides deterministic color assignment for timeline actors.
/// </summary>
/// <remarks>
/// Maps actor identifiers to stable palette colors using hash-based selection.
/// </remarks>
internal static class ActorColorPalette
{
    private static readonly uint[] Hues =
    [
        0x64B5F6,
        0x81C784,
        0xFFB74D,
        0xF06292,
        0x9575CD,
        0x4DB6AC,
        0xFFF176,
        0xFF8A65,
        0x90A4AE,
        0xA1887F,
    ];

    /// <summary>
    /// Gets deterministic display color for a specified actor identifier.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>RGBA color vector assigned to an actor.</returns>
    public static Vector4 ForActor(string actor)
    {
        int hash = StringComparer.OrdinalIgnoreCase.GetHashCode(actor);

        uint rgb = Hues[
            (hash & int.MaxValue) % Hues.Length];

        return ColorPalette.UIntToVec4(rgb);
    }
}