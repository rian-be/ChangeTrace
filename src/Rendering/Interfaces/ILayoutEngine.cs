using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Interfaces;

/// <summary>
/// Computes scene graph node positions.
/// </summary>
internal interface ILayoutEngine
{
    /// <summary>
    /// Advances layout simulation by one step.
    /// </summary>
    void Step(
        IReadOnlyDictionary<string, SceneNode> nodes,
        float deltaSeconds);

    /// <summary>
    /// Current layout system energy.
    /// </summary>
    float Energy { get; }
}