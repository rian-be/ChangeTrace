using System.Runtime.InteropServices;

namespace ChangeTrace.Graphics.Gpu.Contracts;

/// <summary>
/// Represents OpenGL indirect draw command structure for array rendering.
/// </summary>
/// <remarks>
/// Matches memory layout expected by <c>glDrawArraysIndirect</c>.
/// </remarks>
[StructLayout(LayoutKind.Sequential)]
internal struct DrawArraysIndirectCommand
{
    /// <summary>
    /// Number of vertices to render.
    /// </summary>
    public uint Count;

    /// <summary>
    /// Number of instances to render.
    /// </summary>
    public uint InstanceCount;

    /// <summary>
    /// Starting vertex offset.
    /// </summary>
    public uint First;

    /// <summary>
    /// Base instance identifier.
    /// </summary>
    public uint BaseInstance;
}