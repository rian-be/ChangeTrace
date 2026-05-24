using System.Runtime.InteropServices;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents GPU particle instance used for particle rendering.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct GpuParticle
{
    /// <summary>
    /// Particle position in world space.
    /// </summary>
    public GpuVector2 Position;

    /// <summary>
    /// Particle render size.
    /// </summary>
    public float Size;

    /// <summary>
    /// Particle transparency multiplier.
    /// </summary>
    public float Alpha;

    /// <summary>
    /// Particle render color.
    /// </summary>
    public GpuVector4 Color;
}