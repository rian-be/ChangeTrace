using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles branch label lifecycle commands.
/// </summary>
internal sealed class BranchLabelHandler(
    ISceneGraph scene)
    : IRenderCommandHandler
{
    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType => typeof(BranchLabelCommand);

    /// <summary>
    /// Applies branch label command to the scene graph.
    /// </summary>
    public void Handle(RenderCommand command, double virtualTime)
    {
        var cmd = (BranchLabelCommand)command;
    //    Console.WriteLine($"[BranchLabelHandler] Handling branch '{cmd.BranchName}' with Action={cmd.Action} at t={virtualTime:F2}s");

        if (cmd.Action == BranchLabelAction.Appear)
        {
            var existing = scene.FindNode(cmd.BranchName);
            if (existing == null)
            {
                var colorVec = ColorPalette.UIntToVec4(0xFFD54F);
                scene.GetOrAddNode(cmd.BranchName, NodeKind.Branch, RenderingHelpers.RandomNear(), colorVec);
             //   Console.WriteLine($"[BranchLabelHandler] Added branch '{cmd.BranchName}' at t={virtualTime:F2}s");
            }
           // else { Console.WriteLine($"[BranchLabelHandler] Branch '{cmd.BranchName}' already exists at t={virtualTime:F2}s");  }
        }
        else
        {
            scene.RemoveNode(cmd.BranchName);
         //   Console.WriteLine($"[BranchLabelHandler] Removed branch '{cmd.BranchName}' at t={virtualTime:F2}s");
        }
    }
}