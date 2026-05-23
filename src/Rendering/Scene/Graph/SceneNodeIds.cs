using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Scene.Graph;

/// <summary>
/// Utility methods for scene node identifiers.
/// </summary>
internal static class SceneNodeIds
{
    /// <summary>
    /// Normalizes scene node identifier.
    /// </summary>
    public static string Normalize(
        string id,
        NodeKind kind)
    {
        if (kind == NodeKind.Root ||
            id == SceneIds.Root)
        {
            return SceneIds.Root;
        }

        id = id
            .Replace('\\', '/')
            .Trim('/');

        if (!string.IsNullOrWhiteSpace(id))
            return id;

        return kind == NodeKind.Branch
            ? SceneIds.Root
            : id;
    }

    /// <summary>
    /// Resolves parent identifier for a node.
    /// </summary>
    public static string ResolveParentId(
        string id,
        NodeKind kind)
    {
        if (kind == NodeKind.Root)
            return "";

        int lastSlash =
            id.LastIndexOf('/');

        return lastSlash >= 0
            ? id[..lastSlash]
            : "__root_files__";
    }
}