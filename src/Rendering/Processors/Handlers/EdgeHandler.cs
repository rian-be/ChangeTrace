using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="EdgeCommand"/> by adding edges between nodes in scene.
/// </summary>
/// <remarks>
/// Each edge is created with specified kind and timestamp in virtual time.
/// Edge fading and lifetime are managed by <see cref="SceneGraph.TickEdges"/>.
/// </remarks>
internal sealed class EdgeHandler(ISceneGraph scene) : IRenderCommandHandler
{
    /// <summary>
    /// Gets <see cref="RenderCommand"/> type this handler can process.
    /// </summary>
    public Type CommandType => typeof(EdgeCommand);

    /// <summary>
    /// Processes given <see cref="EdgeCommand"/> and adds an edge to scene.
    /// </summary>
    /// <param name="command">The render command to handle.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (EdgeCommand)command;
        scene.AddEdge(cmd.FromNode, cmd.ToNode, cmd.Kind, virtualTime);
    }
}