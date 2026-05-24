using System.Numerics;
using System.Runtime.InteropServices;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents GPU edge instance used for edge rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct GpuEdge
{
    /// <summary>
    /// Edge start position in world space.
    /// </summary>
    public Vector2 From;

    /// <summary>
    /// Edge end position in world space.
    /// </summary>
    public Vector2 To;

    /// <summary>
    /// Edge render color.
    /// </summary>
    public Vector4 Color;

    /// <summary>
    /// Edge transparency multiplier.
    /// </summary>
    public float Alpha;

    /// <summary>
    /// Edge width at the starting point.
    /// </summary>
    public float WidthStart;

    /// <summary>
    /// Edge width at ending point.
    /// </summary>
    public float WidthEnd;

    /// <summary>
    /// Encoded edge classification identifier.
    /// </summary>
    public float Kind;

    /// <summary>
    /// Glow intensity at edge start.
    /// </summary>
    public float FromGlow;

    /// <summary>
    /// Glow intensity at the edge end.
    /// </summary>
    public float ToGlow;

    /// <summary>
    /// Padding for GPU memory alignment.
    /// </summary>
    private Vector2 _padding;
}