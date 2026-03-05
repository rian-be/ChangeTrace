using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

internal sealed class BundledEdgeHandler(ISceneGraph scene) : IRenderCommandHandler
{
    public Type CommandType => typeof(BundledEdgeCommand);

    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (BundledEdgeCommand)command;
        var fromNode = scene.FindNode(cmd.FromNode);
        if (fromNode == null) return;

        var validTargets = cmd.ToNodes
            .Select(id => scene.FindNode(id))
            .Where(n => n != null)
            .Select(n => n!.Id)
            .ToList();

        if (!validTargets.Any()) return;

        //scene.AddBundledEdge(fromNode.Id, validTargets, cmd.Kind, virtualTime);
        Console.WriteLine($"Added BundledEdge from {fromNode.Id} -> {string.Join(", ", validTargets)}");
    }
}