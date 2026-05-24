using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using SysVec4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Rendering.Scene;

/// <summary>
/// Selects the scene nodes that should be rendered for the current zoom level.
/// </summary>
internal sealed class LodPlanner
{
    private const string RootParentId = "__repo_root__";

    private readonly List<NodeSnapshot> _buffer = new();
    private readonly Dictionary<string, int> _filesPerParent = new();

    /// <summary>
    /// Builds the node list for the current frame.
    /// </summary>
    public IReadOnlyList<NodeSnapshot> Build(
        RenderState state,
        float zoom)
    {
        _buffer.Clear();
        _filesPerParent.Clear();

        string? hoveredId =
            state.Hud.Interaction.HoveredNodeId;

        var limits =
            GetLimits(
                zoom);

        int totalFiles =
            0;

        foreach (var node in state.Scene.Nodes)
        {
            bool hovered =
                node.Id == hoveredId;

            if (node.Kind != NodeKind.File)
            {
                AddNode(
                    node,
                    hovered);

                continue;
            }

            bool important =
                hovered ||
                node.Glow > 0.08f;

            if (!important &&
                !CanIncludeFile(
                    node,
                    limits,
                    ref totalFiles))
            {
                continue;
            }

            AddNode(
                node,
                hovered);
        }

        return _buffer;
    }

    /// <summary>
    /// Checks whether a file node fits the active LOD limits.
    /// </summary>
    private bool CanIncludeFile(
        NodeSnapshot node,
        LodLimits limits,
        ref int totalFiles)
    {
        if (limits.MaxFilesPerParent <= 0)
            return false;

        if (totalFiles >= limits.MaxTotalFiles)
            return false;

        string parentId =
            string.IsNullOrWhiteSpace(node.ParentId)
                ? RootParentId
                : node.ParentId;

        int used =
            _filesPerParent.GetValueOrDefault(
                parentId);

        if (used >= limits.MaxFilesPerParent)
            return false;

        _filesPerParent[parentId] =
            used + 1;

        totalFiles++;

        return true;
    }

    /// <summary>
    /// Adds a node to the current visible node buffer.
    /// </summary>
    private void AddNode(
        NodeSnapshot node,
        bool highlighted)
    {
        _buffer.Add(
            highlighted
                ? Highlight(node)
                : node);
    }

    /// <summary>
    /// Returns a highlighted copy of the node.
    /// </summary>
    private static NodeSnapshot Highlight(
        NodeSnapshot node)
    {
        return node with
        {
            Color = new SysVec4(
                0.2f,
                1.0f,
                1.0f,
                1.0f),

            Glow = 2.0f
        };
    }

    /// <summary>
    /// Resolves file rendering limits for the current zoom level.
    /// </summary>
    private static LodLimits GetLimits(
        float zoom)
    {
        return zoom switch
        {
            < 0.35f => new LodLimits(0, 0),
            < 0.75f => new LodLimits(2, 180),
            < 1.25f => new LodLimits(4, 360),
            < 2.25f => new LodLimits(8, 700),
            < 4.00f => new LodLimits(16, 1200),
            < 7.00f => new LodLimits(32, 2000),
            _ => new LodLimits(64, 3500)
        };
    }

    /// <summary>
    /// File visibility limits for a zoom band.
    /// </summary>
    private readonly record struct LodLimits(
        int MaxFilesPerParent,
        int MaxTotalFiles);
}