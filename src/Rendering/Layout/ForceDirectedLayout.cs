using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout.Forces;
using ChangeTrace.Rendering.Layout.Integration;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout;

/// <summary>
/// Layout engine implementing force directed graph algorithm.
/// </summary>
/// <remarks>
/// Uses an <see cref="IForceCalculator"/> to compute forces (repulsion, springs, gravity)
/// and an <see cref="IIntegrator"/> to apply velocity integration and update node positions.
/// </remarks>
internal sealed class ForceDirectedLayout(IForceCalculator calculator, IIntegrator integrator) : ILayoutEngine
{
    /// <summary>
    /// Performs single layout step.
    /// </summary>
    /// <param name="nodes">Scene nodes to layout.</param>
    /// <param name="deltaSeconds">Elapsed time step in seconds.</param>
    public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
    {
        if (nodes.Count < 2) return;

        var nodeList = nodes.Values.ToList();
        var forces = calculator.CalculateForces(nodeList);
        integrator.Integrate(nodeList, forces, deltaSeconds);
    }
}