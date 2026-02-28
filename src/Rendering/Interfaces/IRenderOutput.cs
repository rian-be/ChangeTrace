using ChangeTrace.Rendering.States;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Represents target for rendered scene snapshots.
/// </summary>
/// <remarks>
/// Implementations receive immutable <see cref="RenderState"/> instances
/// produced by the rendering pipeline and may display them, store them, or
/// forward them to other systems (e.g., a UI canvas, video capture, or debugging output).
/// </remarks>
internal interface IRenderOutput
{
    /// <summary>
    /// Submits new <see cref="RenderState"/> for output.
    /// </summary>
    /// <param name="state">The snapshot of scene to render.</param>
    void Submit(RenderState state);
}