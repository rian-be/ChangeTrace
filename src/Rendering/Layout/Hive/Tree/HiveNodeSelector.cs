using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Layout.Hive.Geometry;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Layout.Hive.Tree;

/// <summary>
/// Helpers for selecting and ordering hive child nodes.
/// </summary>
internal static class HiveNodeSelector
{
    /// <summary>
    /// Returns non-file child nodes ordered by stable angle.
    /// </summary>
    public static List<SceneNode> GetDirectories(IReadOnlyList<SceneNode> children)
    {
        return children
            .Where(x => x.Kind != NodeKind.File)
            .OrderBy(x => HiveGeometry.StableAngle(x.Id))
            .ToList();
    }

    /// <summary>
    /// Returns file child nodes ordered by stable angle.
    /// </summary>
    public static List<SceneNode> GetFiles(IReadOnlyList<SceneNode> children)
    {
        return children
            .Where(x => x.Kind == NodeKind.File)
            .OrderBy(x => HiveGeometry.StableAngle(x.Id))
            .ToList();
    }
}