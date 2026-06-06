using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.States.Nodes;
using Xunit;

namespace ChangeTrace.Tests.Rendering.States.Nodes;

/// <summary>Tests CPU-side node snapshot assembly.</summary>
public sealed class NodeSnapshotAssemblerTests
{
    /// <summary>Assemble assigns root, folder, and file snapshot styles without GPU access.</summary>
    [Fact]
    public void Assemble_BuildsSnapshotsWithExpectedParentIdsAndStyles()
    {
        var root = new SceneNode(SceneIds.Root, NodeKind.Root, new Vec2(0, 0));
        var folder = new SceneNode("src", NodeKind.Branch, new Vec2(1, 0)) { IsParent = true };
        var file = new SceneNode("src/Program.cs", NodeKind.File, new Vec2(2, 0)) { Glow = 1f };
        var nodes = new Dictionary<string, SceneNode>
        {
            [root.Id] = root,
            [folder.Id] = folder,
            [file.Id] = file
        };
        var assembler = new NodeSnapshotAssembler();

        var snapshots = assembler.Assemble(nodes);

        var rootSnapshot = snapshots.Single(snapshot => snapshot.Id == SceneIds.Root);
        var folderSnapshot = snapshots.Single(snapshot => snapshot.Id == "src");
        var fileSnapshot = snapshots.Single(snapshot => snapshot.Id == "src/Program.cs");
        Assert.Null(rootSnapshot.ParentId);
        Assert.Equal("__root_files__", folderSnapshot.ParentId);
        Assert.Equal("src", fileSnapshot.ParentId);
        Assert.True(rootSnapshot.Radius > folderSnapshot.Radius);
        Assert.True(fileSnapshot.Glow > 0);
    }
}
