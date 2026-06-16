using System.Numerics;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States.Scene;
using Xunit;

namespace ChangeTrace.Tests.Rendering.States;

/// <summary>Tests immutable scene snapshot materialization behavior.</summary>
public sealed class SceneSnapshotTests
{
    [Fact]
    public void Constructor_SortsNodesAndFindNodeUsesSortedIndex()
    {
        var nodes = new[]
        {
            new NodeSnapshot("src/Program.cs", new Vec2(2, 0), 4f, new Vector4(1f), 0.1f, NodeKind.File, "Program.cs", false, "src"),
            new NodeSnapshot(SceneIds.Root, new Vec2(0, 0), 18f, new Vector4(1f), 1f, NodeKind.Root, SceneIds.RootLabel, true),
            new NodeSnapshot("src", new Vec2(1, 0), 7f, new Vector4(1f), 0.2f, NodeKind.Branch, "src", true, "__root_files__")
        };

        var snapshot = SceneSnapshotMaterializer.Create(nodes, [], [], []);

        Assert.Collection(
            snapshot.Nodes,
            node => Assert.Equal(SceneIds.Root, node.Id),
            node => Assert.Equal("src", node.Id),
            node => Assert.Equal("src/Program.cs", node.Id));

        var branch = snapshot.FindNode("src");
        Assert.NotNull(branch);
        Assert.Equal(NodeKind.Branch, branch.Value.Kind);
    }

    [Fact]
    public void Constructor_RemapsEdgeIndexesAfterNodeSortingAndDropsInvalidDuplicates()
    {
        var nodes = new[]
        {
            new NodeSnapshot("src/Program.cs", new Vec2(2, 0), 4f, new Vector4(1f), 0.1f, NodeKind.File, "Program.cs", false, "src"),
            new NodeSnapshot(SceneIds.Root, new Vec2(0, 0), 18f, new Vector4(1f), 1f, NodeKind.Root, SceneIds.RootLabel, true),
            new NodeSnapshot("src", new Vec2(1, 0), 7f, new Vector4(1f), 0.2f, NodeKind.Branch, "src", true, "__root_files__")
        };

        var edges = new[]
        {
            new EdgeSnapshotIndexed(2, 0, EdgeKind.Hierarchy, 1f, new Vector4(1f)),
            new EdgeSnapshotIndexed(2, 0, EdgeKind.Hierarchy, 1f, new Vector4(1f)),
            new EdgeSnapshotIndexed(9, 0, EdgeKind.Hierarchy, 1f, new Vector4(1f))
        };

        var snapshot = SceneSnapshotMaterializer.Create(nodes, [], edges, []);

        var edge = Assert.Single(snapshot.Edges);
        Assert.Equal("src", snapshot.Nodes[edge.FromIndex].Id);
        Assert.Equal("src/Program.cs", snapshot.Nodes[edge.ToIndex].Id);

        Assert.Equal("src", snapshot.Nodes[snapshot.Edges[0].FromIndex].Id);
        Assert.Equal("src/Program.cs", snapshot.Nodes[snapshot.Edges[0].ToIndex].Id);
    }
}
