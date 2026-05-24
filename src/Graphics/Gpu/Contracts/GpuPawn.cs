using System.Numerics;
using System.Runtime.InteropServices;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents a GPU pawn instance used for actor rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct GpuPawn
{
    /// <summary>
    /// Pawn position in world space.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Pawn render radius.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Pawn transparency multiplier.
    /// </summary>
    public float Alpha;

    /// <summary>
    /// Pawn render color.
    /// </summary>
    public Vector4 Color;

    /// <summary>
    /// Additional emissive intensity multiplier.
    /// </summary>
    public float Glow;

    /// <summary>
    /// Padding for GPU memory alignment.
    /// </summary>
    public Vector3 Padding;
}