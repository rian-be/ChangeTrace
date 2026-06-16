using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States.Edges;
using ChangeTrace.Rendering.States.Nodes;
using Xunit;

namespace ChangeTrace.Tests.Rendering.States.Edges;

/// <summary>Tests CPU-side edge snapshot assembly.</summary>
public sealed class EdgeSnapshotAssemblerTests
{
    private readonly NodeSnapshotAssembler _nodeAssembler = new();

    private static Dictionary<string, int> BuildNodeIndex(
        IReadOnlyList<NodeSnapshot> nodes)
    {
        var nodeIndex = new Dictionary<string, int>(nodes.Count);

        for (var i = 0; i < nodes.Count; i++)
            nodeIndex[nodes[i].Id] = i;

        return nodeIndex;
    }

    /// <summary>Assemble suppresses hierarchy edges to files when a folder has too many file children.</summary>
    [Fact]
    public void Assemble_SuppressesHeavyFileHierarchyEdges()
    {
        var scene = new SceneGraph();
        scene.GetOrCreateRoot();
        scene.GetOrAddNode("src", NodeKind.Branch, new Vec2(0, 0));

        for (var i = 0; i < 18; i++)
            scene.GetOrAddNode($"src/file-{i}.cs", NodeKind.File, new Vec2(i + 1, 0));

        var assembler = new EdgeSnapshotAssembler();
        var nodeSnapshots = _nodeAssembler.Assemble(scene.Nodes);
        var nodeIndex = BuildNodeIndex(nodeSnapshots);

        var snapshots = assembler.Assemble(scene, nodeIndex);

        Assert.Contains(snapshots, snapshot =>
            nodeSnapshots[snapshot.FromIndex].Id == "__root_files__" &&
            nodeSnapshots[snapshot.ToIndex].Id == "src");
        Assert.DoesNotContain(snapshots, snapshot =>
            nodeSnapshots[snapshot.ToIndex].Id.StartsWith("src/file-"));
    }

    /// <summary>Assemble includes file hierarchy edges below the sibling suppression threshold and ignores runtime edges.</summary>
    [Fact]
    public void Assemble_KeepsLightHierarchyEdgesAndIgnoresRuntimeEdges()
    {
        var scene = new SceneGraph();
        scene.GetOrCreateRoot();
        scene.GetOrAddNode("src", NodeKind.Branch, new Vec2(0, 0));
        scene.GetOrAddNode("src/Program.cs", NodeKind.File, new Vec2(1, 0));
        scene.AddEdge(SceneIds.Root, "src/Program.cs", EdgeKind.Commit, virtualTime: 1.0);

        var assembler = new EdgeSnapshotAssembler();
        var nodeSnapshots = _nodeAssembler.Assemble(scene.Nodes);
        var nodeIndex = BuildNodeIndex(nodeSnapshots);

        var snapshots = assembler.Assemble(scene, nodeIndex);

        Assert.Contains(snapshots, snapshot =>
            nodeSnapshots[snapshot.FromIndex].Id == "src" &&
            nodeSnapshots[snapshot.ToIndex].Id == "src/Program.cs");
        Assert.DoesNotContain(snapshots, snapshot => snapshot.Kind == EdgeKind.Commit);
    }
}
