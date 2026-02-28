using ChangeTrace.Core.Models;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Colors;

/// <summary>
/// Provides color mappings for actors, file types, and edge kinds in visualization.
/// </summary>
/// <remarks>
/// - Actor colors are selected from predefined palette based on actor name hash.
/// - File extension colors use fixed mapping.
/// - Edge kinds and special effects have fixed color values.
/// </remarks>
internal static class ColorPalette
{
    private static readonly uint[] ActorHues =
    [
        0x64B5F6, // blue
        0x81C784, // green
        0xFFB74D, // orange
        0xF06292, // pink
        0x9575CD, // purple
        0x4DB6AC, // teal
        0xFFF176, // yellow
        0xFF8A65, // deep orange
        0x90A4AE, // blue-grey
        0xA1887F, // brown
    ];

    /// <summary>
    /// Gets color for given actor.
    /// </summary>
    /// <param name="actor">Actor identifier.</param>
    /// <returns>Packed RGB color value.</returns>
    internal static uint ForActor(ActorName actor)
    {
        int hash = Math.Abs(actor.Value.GetHashCode());
        return ActorHues[hash % ActorHues.Length];
    }

    private static readonly Dictionary<string, uint> ExtensionColors = new(StringComparer.OrdinalIgnoreCase)
    {
        [".cs"]   = 0x7B68EE,  // slate blue
        [".ts"]   = 0x4169E1,  // royal blue
        [".js"]   = 0xF0DB4F,  // JS yellow
        [".py"]   = 0x4B8BBE,  // python blue
        [".go"]   = 0x00ACD7,  // go cyan
        [".rs"]   = 0xDEA584,  // rust orange
        [".java"] = 0xED8B00,  // java orange
        [".cpp"]  = 0x00599C,  // C++ blue
        [".c"]    = 0x555555,  // C grey
        [".md"]   = 0xFFFFFF,  // white (docs)
        [".json"] = 0xCBCB41,  // olive
        [".yaml"] = 0xCB6F29,  // yaml orange
        [".yml"]  = 0xCB6F29,
        [".xml"]  = 0x0060AC,
        [".html"] = 0xE34C26,  // html orange-red
        [".css"]  = 0x264DE4,  // CSS blue
        [".sh"]   = 0x89E051,  // shell green
        [".sql"]  = 0xE38C00,
    };

    /// <summary>
    /// Gets color for file path based on extension.
    /// </summary>
    /// <param name="path">File path.</param>
    /// <returns>Packed RGB color value; fallback is gray if unknown.</returns>
    internal static uint ForFilePath(string path)
    {
        var ext = Path.GetExtension(path);
        if (ExtensionColors.TryGetValue(ext, out var color))
            return color;
        return 0x888888u;
    }

    /// <summary>
    /// Gets color for given edge kind.
    /// </summary>
    /// <param name="kind">Edge classification.</param>
    /// <returns>Packed RGB color value.</returns>
    internal static uint ForEdge(EdgeKind kind) => kind switch
    {
        EdgeKind.Commit => 0x4FC3F7,  // light blue
        EdgeKind.Merge => 0xA5D6A7,  // light green
        EdgeKind.PullRequest => 0xCE93D8,  // light purple
        _ => 0xAAAAAA
    };

    /// <summary>
    /// Color for merge particle effects.
    /// </summary>
    internal static uint MergeParticle => 0xA5D6A7;

    /// <summary>
    /// Color for pull request particle effects.
    /// </summary>
    internal static uint PrParticle => 0xCE93D8;

    /// <summary>
    /// Glow color for scene nodes.
    /// </summary>
    internal static uint NodeGlow => 0xFFFFFF;

    private static (byte R, byte G, byte B) Unpack(uint rgb) =>
        ((byte)(rgb >> 16), (byte)(rgb >> 8 & 0xFF), (byte)(rgb & 0xFF));

    /// <summary>
    /// Linearly interpolates between two RGB colors.
    /// </summary>
    /// <param name="a">Start color.</param>
    /// <param name="b">End color.</param>
    /// <param name="t">Interpolation factor [0,1].</param>
    /// <returns>Interpolated packed RGB color.</returns>
    internal static uint Lerp(uint a, uint b, float t)
    {
        var (ar, ag, ab) = Unpack(a);
        var (br, bg, bb) = Unpack(b);
        byte r = (byte)(ar + (br - ar) * t);
        byte g = (byte)(ag + (bg - ag) * t);
        byte bl = (byte)(ab + (bb - ab) * t);
        return (uint)(r << 16 | g << 8 | bl);
    }
}