using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Layout;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering;

internal sealed class GourceStyleLayout(ForceDirectedLayout inner, float speedX) : ILayoutEngine
{
    public void Step(IReadOnlyDictionary<string, SceneNode> nodes, float deltaSeconds)
    {
     //   Console.WriteLine($"Step: {nodes.Count} nodes, deltaSeconds={deltaSeconds}");
        
        inner.Step(nodes, deltaSeconds);

        // przesuwanie w osi X (Gource)
        foreach (var node in nodes.Values)
        {
            var oldX = node.Position.X;
            node.Position = node.Position with { X = oldX + speedX * deltaSeconds };

         //   Console.WriteLine($"Node {node.Id}: X {oldX:F2} -> {node.Position.X:F2}");
        }
    }
}