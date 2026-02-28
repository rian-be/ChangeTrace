using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Proximity;

/// <summary>
/// Proximity strategy that connects nodes sharing same directory path.
/// </summary>
/// <remarks>
/// Nodes are considered connected if their identifiers contain common
/// parent directory (based on the substring before the last '/').
/// Intended primarily for file based layouts where node IDs represent paths.
/// </remarks>
internal sealed class DirectoryProximity : INodeProximity
{
    /// <summary>
    /// Determines whether two nodes belong to same directory.
    /// </summary>
    /// <param name="a">First scene node.</param>
    /// <param name="b">Second scene node.</param>
    /// <returns>
    /// <c>true</c> if both node IDs share same parent directory;
    /// otherwise, <c>false</c>.
    /// </returns>
    public bool AreConnected(SceneNode a, SceneNode b)
    {
        var slashA = a.Id.LastIndexOf('/');
        var slashB = b.Id.LastIndexOf('/');
        if (slashA < 0 || slashB < 0) 
            return false;

        return a.Id.AsSpan(0, slashA)
            .SequenceEqual(b.Id.AsSpan(0, slashB));
    }
}