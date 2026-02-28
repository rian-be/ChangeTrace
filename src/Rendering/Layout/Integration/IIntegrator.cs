using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Integration;

/// <summary>
/// Defines physics integration strategy for updating node positions.
/// </summary>
/// <remarks>
/// Implementations apply accumulated forces to <see cref="SceneNode"/> instances
/// and advance their positions over time using a specific numerical integration
/// </remarks>
internal interface IIntegrator
{
    /// <summary>
    /// Integrates forces and updates node positions for single simulation step.
    /// </summary>
    /// <param name="nodes">Collection of scene nodes to update.</param>
    /// <param name="forces">
    /// Accumulated force vectors per node identifier.
    /// Keys must correspond to node IDs.
    /// </param>
    /// <param name="deltaSeconds">
    /// Elapsed simulation time in seconds since previous integration step.
    /// </param>
    void Integrate(
        IReadOnlyList<SceneNode> nodes,
        Dictionary<string, Vec2> forces,
        float deltaSeconds);
}