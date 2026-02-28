using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles <see cref="BranchLabelCommand"/> by adding or removing branch node s in scene.
/// </summary>
/// <remarks>
/// When a branch appears,  new node is added at a random offset.
/// When a branch disappears, corresponding node is removed.
/// </remarks>
internal sealed class BranchLabelHandler(ISceneGraph scene) : IRenderCommandHandler
{
    /// <summary>
    /// Returns <see cref="RenderCommand"/> type this handler supports.
    /// </summary>
    public Type CommandType => typeof(BranchLabelCommand);

    /// <summary>
    /// Processes given <see cref="BranchLabelCommand"/> and updates scene.
    /// </summary>
    /// <param name="command">The render command to handle.</param>
    /// <param name="virtualTime">Current virtual timeline time (seconds).</param>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (BranchLabelCommand)command;

        if (cmd.Action == BranchLabelAction.Appear)
            scene.GetOrAddNode(cmd.BranchName, NodeKind.Branch, RenderingHelpers.RandomNear(), 0xFFD54F);
        else
            scene.RemoveNode(cmd.BranchName);
    }
}