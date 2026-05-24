using ChangeTrace.Graphics.Rendering.Resources;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering;

/// <summary>
/// Plans visible labels for the current scene.
/// </summary>
internal sealed class LabelVisibilityPlanner
{
    /// <summary>
    /// Maximum number of non-hover labels allowed on screen.
    /// </summary>
    private const int MaxLabels = 180;

    /// <summary>
    /// Extra off-screen margin used before rejecting labels.
    /// </summary>
    private const float ScreenPadding = 24f;

    /// <summary>
    /// Priority values used to keep important labels visible.
    /// </summary>
    private const float RootPriority = 100000f;
    private const float HoverPriority = 1000000f;

    /// <summary>
    /// Minimum zoom levels required for automatic labels.
    /// </summary>
    private const float FolderMinZoom = 0.15f;
    private const float FileMinZoom = 1.35f;

    /// <summary>
    /// Screen-space label offsets.
    /// </summary>
    private const float RootOffsetY = -34f;
    private const float FolderOffsetY = -26f;
    private const float FileOffsetY = 14f;

    /// <summary>
    /// Extra label bounds padding used for collision checks.
    /// </summary>
    private const float CollisionPaddingX = 10f;
    private const float CollisionPaddingY = 6f;

    /// <summary>
    /// Screen-space offset used by hovered labels.
    /// </summary>
    private static readonly Vector2 HoverOffset =
        new(14f, -30f);

    private readonly List<PlannedLabel> _labels = [];
    private readonly List<PlannedLabel> _result = [];
    private readonly List<Rect> _occupied = [];

    /// <summary>
    /// Builds the visible label list for the current camera frame.
    /// </summary>
    public IReadOnlyList<PlannedLabel> Plan(
        ISceneSnapshot scene,
        CameraSnapshot camera,
        int viewportWidth,
        int viewportHeight,
        string? hoveredNodeId,
        HoveredPodHud? hoveredPod)
    {
        _labels.Clear();
        _result.Clear();
        _occupied.Clear();

        if (scene.IsEmpty)
            return _result;

        Matrix3 viewProj = ViewProjection.Build(
                camera.Position,
                camera.Zoom,
                camera.Rotation,
                viewportWidth,
                viewportHeight);

        foreach (NodeSnapshot node in scene.Nodes)
        {
            bool hovered = !string.IsNullOrWhiteSpace(hoveredNodeId) &&
                node.Id == hoveredNodeId;

            if (!ShouldConsider(node, camera.Zoom, hovered))
            {
                continue;
            }

            if (!TryProjectToScreen(
                    node.Position,
                    viewProj,
                    viewportWidth,
                    viewportHeight,
                    out Vector2 screenPosition))
            {
                continue;
            }

            if (!IsNearScreen( screenPosition, viewportWidth, viewportHeight))
            {
                continue;
            }

            string text = GetLabelText(node);

            if (string.IsNullOrWhiteSpace(text))
                continue;

            float score = CalculateScore(node, camera.Zoom, hovered);
            Vector2 anchor = GetAnchorPosition(node, screenPosition, hovered);

            bool persistent =
                hovered || node.Kind == NodeKind.Root ||
                node.Kind != NodeKind.File;

            _labels.Add(
                new PlannedLabel(
                    text,
                    anchor,
                    node.Kind,
                    score,
                    hovered,
                    persistent,
                    IsPod: false));
        }

        _labels.Sort(
            static (a, b) => b.Score.CompareTo(a.Score));

        foreach (PlannedLabel label in _labels)
        {
            if (_result.Count >= MaxLabels &&
                !label.IsHovered)
            {
                continue;
            }

            Rect bounds =
                EstimateBounds(label);

            if (!label.IsHovered &&
                Collides(bounds))
            {
                continue;
            }

            _result.Add(label);
            _occupied.Add(bounds);
        }

        return _result;
    }

    /// <summary>
    /// Checks whether a node is eligible for label generation.
    /// </summary>
    private static bool ShouldConsider(
        NodeSnapshot node,
        float zoom,
        bool hovered)
    {
        if (hovered)
            return true;

        if (node.Kind == NodeKind.Root)
            return true;

        if (node.Kind != NodeKind.File)
            return zoom >= FolderMinZoom;

        return zoom >= FileMinZoom || node.Glow > 0.12f;
    }

    /// <summary>
    /// Projects world position into screen space.
    /// </summary>
    private static bool TryProjectToScreen(
        Vec2 worldPosition,
        Matrix3 viewProj,
        int viewportWidth,
        int viewportHeight,
        out Vector2 screenPosition)
    {
        float ndcX =
            viewProj.M11 * worldPosition.X +
            viewProj.M21 * worldPosition.Y +
            viewProj.M31;

        float ndcY =
            viewProj.M12 * worldPosition.X +
            viewProj.M22 * worldPosition.Y +
            viewProj.M32;

        screenPosition =
            new Vector2(
                (ndcX + 1f) * 0.5f * viewportWidth,
                (1f - ndcY) * 0.5f * viewportHeight);

        return
            !float.IsNaN(screenPosition.X) &&
            !float.IsNaN(screenPosition.Y) &&
            !float.IsInfinity(screenPosition.X) &&
            !float.IsInfinity(screenPosition.Y);
    }

    /// <summary>
    /// Checks whether a projected label anchor is close enough to the viewport.
    /// </summary>
    private static bool IsNearScreen(
        Vector2 screenPosition,
        int viewportWidth,
        int viewportHeight)
    {
        return
            screenPosition is { X: >= -ScreenPadding, Y: >= -ScreenPadding } &&
            screenPosition.X <= viewportWidth + ScreenPadding &&
            screenPosition.Y <= viewportHeight + ScreenPadding;
    }

    /// <summary>
    /// Resolves label anchor position for a node.
    /// </summary>
    private static Vector2 GetAnchorPosition(
        NodeSnapshot node,
        Vector2 nodeScreenPosition,
        bool hovered)
    {
        if (hovered)
        {
            /*
             * Important:
             * offset is applied relative to the node position,
             * not the cursor position.
             */
            return nodeScreenPosition + HoverOffset;
        }

        return node.Kind switch
        {
            NodeKind.Root =>
                nodeScreenPosition + new Vector2(0f, RootOffsetY),

            NodeKind.File =>
                nodeScreenPosition + new Vector2(8f, FileOffsetY),

            _ =>
                nodeScreenPosition + new Vector2(0f, FolderOffsetY)
        };
    }

    /// <summary>
    /// Calculates label priority used during collision selection.
    /// </summary>
    private static float CalculateScore(
        NodeSnapshot node,
        float zoom,
        bool hovered)
    {
        if (hovered)
            return HoverPriority;

        if (node.Kind == NodeKind.Root)
            return RootPriority;

        float kindScore =
            node.Kind switch
            {
                NodeKind.File => 100f,
                _ => 5000f
            };

        float glowScore =
            node.Glow * 12000f;

        float zoomScore =
            MathF.Min(
                12000f,
                zoom * 2500f);

        return kindScore + glowScore + zoomScore;
    }

    /// <summary>
    /// Estimates screen-space label bounds.
    /// </summary>
    private static Rect EstimateBounds(
        PlannedLabel label)
    {
        float scale =
            label.IsHovered
                ? 1.18f
                : label.Kind == NodeKind.File
                    ? 0.92f
                    : 1.05f;

        float width =
            label.Text.Length * 8f * scale;

        float height =
            14f * scale;

        return new Rect(
            label.ScreenPosition.X - CollisionPaddingX,
            label.ScreenPosition.Y - CollisionPaddingY,
            label.ScreenPosition.X + width + CollisionPaddingX,
            label.ScreenPosition.Y + height + CollisionPaddingY);
    }

    /// <summary>
    /// Checks whether bounds collide with accepted labels.
    /// </summary>
    private bool Collides(Rect rect)
    {
        foreach (var  other in _occupied)
        {
            if (rect.Intersects(other))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Resolves label text for a node.
    /// </summary>
    private static string GetLabelText(
        NodeSnapshot node)
    {
        return node.Label;
    }

    internal readonly record struct PlannedLabel(
        string Text,
        Vector2 ScreenPosition,
        NodeKind Kind,
        float Score,
        bool IsHovered,
        bool IsPersistent,
        bool IsPod);

    private readonly record struct Rect(
        float Left,
        float Top,
        float Right,
        float Bottom)
    {
        public bool Intersects(Rect other)
        {
            return
                Left < other.Right &&
                Right > other.Left &&
                Top < other.Bottom &&
                Bottom > other.Top;
        }
    }
}