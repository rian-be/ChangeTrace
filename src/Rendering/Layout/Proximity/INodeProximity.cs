using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Proximity;

/// <summary>
/// Defines strategy for determining logical proximity between two scene nodes.
/// </summary>
/// <remarks>
/// Implementations decide whether two <see cref="SceneNode"/> instances should
/// be considered connected for layout purposes (e.g., force directed attraction,
/// clustering, or edge based constraints).
/// </remarks>
internal interface INodeProximity
{
    /// <summary>
    /// Determines whether two nodes are logically connected.
    /// </summary>
    /// <param name="a">First node.</param>
    /// <param name="b">Second node.</param>
    /// <returns>
    /// <c>true</c> if nodes should be treated as connected in layout;
    /// otherwise, <c>false</c>.
    /// </returns>
    bool AreConnected(SceneNode a, SceneNode b);
}