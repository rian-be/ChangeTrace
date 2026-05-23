using System.Numerics;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;

namespace ChangeTrace.Rendering.States.Nodes;

/// <summary>
/// Builds immutable node snapshots for rendering.
/// </summary>
internal sealed class NodeSnapshotAssembler
{
    /// <summary>
    /// Converts live scene nodes into render snapshots.
    /// </summary>
    public List<NodeSnapshot> Assemble(
        IReadOnlyDictionary<string, SceneNode> nodes)
    {
        var nodeCount = nodes.Count;

        var nodeDensityScale =
            MathF.Max(
                0.45f,
                1.0f - nodeCount / 8000f);

        var nodesArray =
            nodes.Values.ToArray();

        var snapshots =
            new NodeSnapshot[nodesArray.Length];

        Parallel.For(
            0,
            nodesArray.Length,
            i =>
            {
                var node = nodesArray[i];

                var style =
                    NodeSnapshotStyle.FromNode(
                        node,
                        nodeCount,
                        nodeDensityScale);

                var parentId =
                    node.Kind == NodeKind.Root
                        ? null
                        : string.IsNullOrWhiteSpace(node.ParentId)
                            ? SceneIds.Root
                            : node.ParentId;

                snapshots[i] =
                    new NodeSnapshot(
                        node.Id,
                        node.Position,
                        style.Radius,
                        style.Color,
                        style.Glow,
                        node.Flyweight,
                        node.Label,
                        node.IsParent,
                        parentId);
            });

        return [..snapshots];
    }

    /// <summary>
    /// Render style values derived from node type and activity.
    /// </summary>
    private readonly record struct NodeSnapshotStyle(
        float Radius,
        Vector4 Color,
        float Glow)
    {
        /// <summary>
        /// Creates snapshot styling for a scene node.
        /// </summary>
        public static NodeSnapshotStyle FromNode(
            SceneNode node,
            int nodeCount,
            float nodeDensityScale)
        {
            var glow = node.Glow;
            var color = node.CachedColor;

            if (node.Kind == NodeKind.File)
            {
                var pulse =
                    1.0f + node.Glow * 1.5f;

                var radius =
                    MathF.Max(
                        2.2f,
                        4.5f * nodeDensityScale * pulse);

                glow *= 0.35f;

                color = color with { W = 0.9f };

                return new NodeSnapshotStyle(
                    radius,
                    color,
                    glow);
            }

            if (node.Kind == NodeKind.Root)
            {
                return new NodeSnapshotStyle(
                    Radius: 18f,
                    Color: new Vector4(
                        1.0f,
                        0.85f,
                        0.15f,
                        1.0f),
                    Glow: 1.0f);
            }

            var folderRadius =
                7f +
                MathF.Min(
                    16f,
                    MathF.Sqrt(nodeCount) * 0.08f);

            var folderGlow =
                0.35f + node.Glow * 0.4f;

            var folderColor =
                color with { W = 0.95f };

            return new NodeSnapshotStyle(
                folderRadius,
                folderColor,
                folderGlow);
        }
    }
}