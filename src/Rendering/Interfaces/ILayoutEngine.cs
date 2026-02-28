using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Contract for layout engines that compute node positions in scene graph.
/// </summary>
/// <remarks>
/// Implementations of <see cref="ILayoutEngine"/> are responsible for arranging
/// <see cref="SceneNode"/> instances in 2D space each frame, typically to reduce
/// overlap, maintain visual hierarchy, or apply force-directed layouts.
/// </remarks>
internal interface ILayoutEngine
{
    /// <summary>
    /// Advances layout computation by one simulation step.
    /// </summary>
    /// <param name="nodes">Current set of nodes in scene.</param>
    /// <param name="deltaSeconds">Elapsed time since last step, for smooth movement or damping calculations.</param>
    void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds);
}