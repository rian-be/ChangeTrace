using ChangeTrace.Rendering.Scene;
using System;

namespace ChangeTrace.Rendering.Layout.Proximity;

/// <summary>
/// Proximity strategy that connects nodes in the same directory.
/// </summary>
internal sealed class DirectoryProximityGource : INodeProximity
{
    public bool AreConnected(SceneNode a, SceneNode b)
    {
        var slashA = a.Id.LastIndexOf('/');
        var slashB = b.Id.LastIndexOf('/');
        if (slashA < 0 || slashB < 0)
            return false;

        return a.Id.AsSpan(0, slashA).SequenceEqual(b.Id.AsSpan(0, slashB));
    }

    /// <summary>
    /// Returns the depth of the node in folder hierarchy.
    /// Root = 0, each subfolder increments depth.
    /// </summary>
    public static int GetDepth(SceneNode node)
    {
        return node.Id.Count(c => c == '/');
    }
}