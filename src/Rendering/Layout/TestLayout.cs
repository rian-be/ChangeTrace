using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

internal sealed class TestLayout : ILayoutEngine
{
    public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
    {
        var allNodes = nodes.Values.ToList();

        var rootNodes   = allNodes.Where(n => n.Kind == NodeKind.Root).ToList();
        var branchNodes = allNodes.Where(n => n.Kind == NodeKind.Branch).ToList();
        var fileNodes   = allNodes.Where(n => n.Kind == NodeKind.File).ToList();

        //Console.WriteLine($"Roots: {rootNodes.Count}, Branches: {branchNodes.Count}, Files: {fileNodes.Count}");
    }
}