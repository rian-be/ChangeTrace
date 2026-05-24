using System.Numerics;
using System.Runtime.InteropServices;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents GPU-side circle node instance used for indirect or instanced rendering.
/// </summary>
/// <remarks>
/// This structure is uploaded to GPU storage buffers and consumed directly by shaders.
/// Layout and packing must remain stable to match GLSL/std430 expectations.
/// </remarks>
[StructLayout(LayoutKind.Sequential, Pack = 4)]
internal struct GpuCircleNode
{
    /// <summary>
    /// World-space or screen-space position of the circle center.
    /// </summary>
    public Vector2 Position;

    /// <summary>
    /// Circle radius in render units.
    /// </summary>
    public float Radius;

    /// <summary>
    /// Semantic node type or rendering kind identifier.
    /// Used by shaders to branch rendering behavior or styling.
    /// </summary>
    public float Kind;

    /// <summary>
    /// RGBA color used for rendering the node.
    /// </summary>
    public Vector4 Color;

    /// <summary>
    /// Additional glow or emissive intensity applied during rendering.
    /// </summary>
    public float Glow;

    /// <summary>
    /// Reserved padding value for GPU memory alignment.
    /// </summary>
    public float Pad0;
    public float Pad1;
    public float Pad2;
}