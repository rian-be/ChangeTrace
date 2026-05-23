using System.Numerics;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles edge creation commands.
/// </summary>
internal sealed class EdgeHandler(
    ISceneGraph scene)
    : IRenderCommandHandler
{
    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType =>
        typeof(EdgeCommand);

    /// <summary>
    /// Applies edge command to the scene graph.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (EdgeCommand)command;
        var fromNode = scene.FindNode(cmd.FromNode) 
                       ?? scene.GetOrAddNode(cmd.FromNode, NodeKind.Root, new Vec2(0,0), new Vector4(1,1,0,1));

        var toNode = scene.FindNode(cmd.ToNode) 
                     ?? scene.GetOrAddNode(cmd.ToNode, NodeKind.File, new Vec2(0,0), new Vector4(1,1,1,1));

        scene.AddEdge(fromNode.Id, toNode.Id, cmd.Kind, virtualTime);
    }
}