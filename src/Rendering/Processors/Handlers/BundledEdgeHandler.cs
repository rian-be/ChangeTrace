using System.Numerics;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles bundled edge creation commands.
/// </summary>
internal sealed class BundledEdgeHandler(
    ISceneGraph scene)
    : IRenderCommandHandler
{
    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType => typeof(BundledEdgeCommand);

    /// <summary>
    /// Applies the bundled edge command to the scene graph.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (BundledEdgeCommand)command;
        
        var fromNode = scene.FindNode(cmd.FromNode) ?? scene.GetOrAddNode(
            cmd.FromNode, 
            NodeKind.Root, 
            new Vec2(0, 0), 
            new Vector4(1, 1, 0, 1));

        var validTargets = new List<string>();
        
        foreach (var targetId in cmd.ToNodes)
        {
            var targetNode = scene.FindNode(targetId) ?? scene.GetOrAddNode(
                targetId,
                NodeKind.File,
                new Vec2(0, 0),
                new Vector4(1, 1, 1, 1));
            validTargets.Add(targetNode.Id);
        }

        if (validTargets.Count == 0)
            return;

        scene.AddBundledEdge(fromNode.Id, validTargets, cmd.Kind, virtualTime);
    }
}