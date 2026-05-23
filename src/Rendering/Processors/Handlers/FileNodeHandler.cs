using System.Numerics;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Rendering.Animation;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Commands;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Helpers;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Scene;

namespace ChangeTrace.Rendering.Processors.Handlers;

/// <summary>
/// Handles file node lifecycle and glow animations.
/// </summary>
internal sealed class FileNodeHandler(
    ISceneGraph scene,
    IAnimationSystem anim,
    IDiagnosticsProvider diagnostics)
    : IRenderCommandHandler
{
    private readonly IDiagnosticsProvider _diagnostics =
        diagnostics;

    /// <summary>
    /// Duration of node glow pulse animation.
    /// </summary>
    private float NodeGlowDuration { get; set; } =
        0.8f;

    /// <summary>
    /// Supported render command type.
    /// </summary>
    public Type CommandType =>
        typeof(FileNodeCommand);

    /// <summary>
    /// Applies file node command to the scene graph.
    /// </summary>
    public void Handle(
        RenderCommand command,
        double virtualTime)
    {
        FileNodeCommand cmd =
            (FileNodeCommand)command;

        string filePath =
            cmd.FilePath.TrimStart('/');

        Vector4 colorVec =
            ColorPalette.ForFilePath(filePath);

        EnsureDirectoryStructure(filePath);

        string parentFolderId =
            PathHelper.GetParentPath(filePath);

        SceneNode? parentNode =
            scene.FindNode(parentFolderId);

        Vec2 spawnPos =
            parentNode?.Position ?? Vec2.Zero;

        switch (cmd.Action)
        {
            case FileNodeAction.Spawn:
            case FileNodeAction.Pulse:
            {
                SceneNode node =
                    scene.GetOrAddNode(
                        cmd.FilePath,
                        NodeKind.File,
                        spawnPos +
                        RenderingHelpers.RandomNear() * 10f,
                        colorVec);

                node.ParentId =
                    parentFolderId;

                if (cmd.Author != null)
                    node.LastAuthor = cmd.Author;

                if (cmd.CommitSha != null)
                    node.LastCommit = cmd.CommitSha;

                _diagnostics.RecordEvent(
                    "Hotspots",
                    cmd.FilePath);

                PulseGlow(node);
                break;
            }

            case FileNodeAction.Remove:
            {
                scene.RemoveNode(filePath);
                break;
            }
        }
    }

    /// <summary>
    /// Ensures parent branch nodes exist for the file path.
    /// </summary>
    private void EnsureDirectoryStructure(string filePath)
    {
        string parentId =
            PathHelper.GetParentPath(filePath);

        scene.GetOrAddNode(
            "",
            NodeKind.Root,
            Vec2.Zero,
            new Vector4(0.05f, 0.05f, 0.08f, 0.8f));

        while (true)
        {
            SceneNode? parentNode =
                scene.FindNode(parentId);

            if (parentNode == null)
            {
                string grandParentId =
                    PathHelper.GetParentPath(parentId);

                SceneNode? grandParentNode =
                    scene.FindNode(grandParentId);

                Vec2 spawnPos =
                    grandParentNode?.Position ??
                    Vec2.Zero;

                SceneNode branch =
                    scene.GetOrAddNode(
                        parentId,
                        NodeKind.Branch,
                        spawnPos +
                        RenderingHelpers.RandomNear() * 20f,
                        new Vector4(1f, 1f, 1f, 0.15f));

                branch.ParentId =
                    grandParentId;
            }

            if (parentId == "")
                break;

            parentId =
                PathHelper.GetParentPath(parentId);
        }
    }

    /// <summary>
    /// Triggers glow pulse animation for the node.
    /// </summary>
    private void PulseGlow(
        SceneNode node,
        float targetGlow = 1f,
        uint? color = null)
    {
        if (color.HasValue)
            node.Color = UIntToVec4(color.Value);

        node.Glow =
            targetGlow;

        try
        {
            anim.TweenFloat(
                targetGlow,
                0f,
                NodeGlowDuration,
                Easing.EaseOutQuad,
                g => node.Glow = g);
        }
        catch (Exception ex)
        {
            Console.WriteLine(
                $"[AnimationSystem][ERROR] Exception creating tween for node {node.Id}: {ex}");
        }
    }

    /// <summary>
    /// Converts packed RGB integer into a normalized color vector.
    /// </summary>
    private static Vector4 UIntToVec4(uint rgb)
    {
        float r =
            ((rgb >> 16) & 0xFF) / 255f;

        float g =
            ((rgb >> 8) & 0xFF) / 255f;

        float b =
            (rgb & 0xFF) / 255f;

        return new Vector4(r, g, b, 1f);
    }
}