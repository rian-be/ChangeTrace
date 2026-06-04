using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Scene;
using ChangeTrace.Rendering.Scene.Relations;
using ChangeTrace.Rendering.States.Edges;
using Xunit;

namespace ChangeTrace.Tests.Rendering.States.Edges;

/// <summary>Tests CPU-side hierarchy edge visibility rules.</summary>
public sealed class EdgeVisibilityFilterTests
{
    /// <summary>ShouldInclude suppresses file hierarchy edges when a folder has too many file children.</summary>
    [Fact]
    public void ShouldInclude_SuppressesHeavyFileSiblingEdges()
    {
        var filter = new EdgeVisibilityFilter();
        var folder = new SceneNode("src", NodeKind.Branch, new Vec2(0, 0));
        var file = new SceneNode("src/Program.cs", NodeKind.File, new Vec2(1, 0));
        var edge = new SceneEdge(folder.Id, file.Id, EdgeKind.Hierarchy, createdAt: 0);

        var include = filter.ShouldInclude(edge, folder, file, new Dictionary<string, int>
        {
            [folder.Id] = 18
        });

        Assert.False(include);
    }

    /// <summary>ShouldInclude keeps folder hierarchy edges regardless of file sibling counts.</summary>
    [Fact]
    public void ShouldInclude_KeepsDirectoryEdges()
    {
        var filter = new EdgeVisibilityFilter();
        var root = new SceneNode(SceneIds.Root, NodeKind.Root, new Vec2(0, 0));
        var folder = new SceneNode("src", NodeKind.Branch, new Vec2(1, 0));
        var edge = new SceneEdge(root.Id, folder.Id, EdgeKind.Hierarchy, createdAt: 0);

        var include = filter.ShouldInclude(edge, root, folder, new Dictionary<string, int>
        {
            [root.Id] = 100
        });

        Assert.True(include);
    }
}
