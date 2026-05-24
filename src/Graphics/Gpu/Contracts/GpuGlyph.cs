using System.Numerics;
using System.Runtime.InteropServices;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents a single GPU-rendered bitmap font glyph instance.
/// </summary>
/// <remarks>
/// One instance corresponds to one rendered character.
/// The vertex shader expands this instance into a quad.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct GpuGlyph
{
    /// <summary>
    /// Glyph origin position in world/screen space.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Glyph render size in world/screen units.
    /// </summary>
    public Vector2 Size;

    /// <summary>
    /// Glyph render color.
    /// </summary>
    public Vector4 Color;

    /// <summary>
    /// Glyph index inside bitmap atlas.
    /// For ASCII atlas this is rawChar - 32.
    /// </summary>
    public int GlyphIndex;

    /// <summary>
    /// Whether vertical placement/UV handling should be flipped.
    /// 0 = normal, 1 = flipped.
    /// </summary>
    public int FlipY;

    /// <summary>
    /// Optional effect/layer identifier.
    /// 0 = normal, 1 = shadow, 2 = outline.
    /// </summary>
    public int Kind;

    /// <summary>
    /// Padding for GPU-friendly layout.
    /// </summary>
    public int Pad0;
}