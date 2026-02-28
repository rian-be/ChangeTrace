using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Forces;

/// <summary>
/// Defines strategy for computing force vectors applied to scene nodes.
/// </summary>
/// <remarks>
/// Implementations accumulate physical forces (e.g., repulsion, attraction,
/// centering, clustering) and return a force vector per node identifier.
/// The resulting force map is typically consumed by an <see cref="Integration.IIntegrator"/>
/// during the simulation step.
/// </remarks>
internal interface IForceCalculator
{
    /// <summary>
    /// Calculates accumulated forces for provided nodes.
    /// </summary>
    /// <param name="nodes">
    /// The collection of scene nodes participating in layout simulation.
    /// </param>
    /// <returns>
    /// A dictionary mapping node IDs to their computed force vectors.
    /// Each node in <paramref name="nodes"/> should have corresponding entry.
    /// </returns>
    Dictionary<string, Vec2> CalculateForces(IReadOnlyList<SceneNode> nodes);
}