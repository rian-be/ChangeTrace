using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Interfaces;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.Interfaces;
using ChangeTrace.Rendering.Snapshots;
using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Text;

/// <summary>
/// Renders planned scene labels using GPU-driven bitmap text rendering.
/// </summary>
internal sealed class LabelRenderer(
    TextGpuPipeline textGpu,
    TextRenderer textRenderer,
    ComputeShader textComputeShader,
    ShaderProgram textRenderShader)
    : ILabelRenderer
{
    /// <summary>
    /// Label rendering scales.
    /// </summary>
    private const float FolderLabelScale = 1.05f;
    private const float FileLabelScale = 0.92f;
    private const float HoverLabelScale = 1.18f;
    private const float PodLabelScale = 1.16f;

    /// <summary>
    /// Maximum label widths in screen-space pixels.
    /// </summary>
    private const float MaxFolderLabelWidth = 240f;
    private const float MaxFileLabelWidth = 260f;
    private const float MaxHoverLabelWidth = 520f;
    private const float MaxPodLabelWidth = 360f;

    /// <summary>
    /// Label color palette.
    /// </summary>
    private static readonly Vector4 RootColor =
        new(1.0f, 0.86f, 0.28f, 1.0f);

    private static readonly Vector4 FolderColor =
        new(0.86f, 0.90f, 1.0f, 0.95f);

    private static readonly Vector4 LargeFolderColor =
        new(0.55f, 0.86f, 1.0f, 1.0f);

    private static readonly Vector4 FileColor =
        new(0.82f, 0.86f, 0.90f, 0.88f);

    private static readonly Vector4 HoverColor =
        new(0.20f, 1.0f, 1.0f, 1.0f);

    private static readonly Vector4 PodColor =
        new(0.64f, 1.0f, 0.95f, 1.0f);

    private static readonly Vector4 ShadowColor =
        new(0.0f, 0.0f, 0.0f, 0.80f);

    private readonly LabelVisibilityPlanner _planner =
        new();

    /// <summary>
    /// Draws visible labels for the current frame.
    /// </summary>
    public void Draw(
        ISceneSnapshot scene,
        CameraSnapshot camera,
        Matrix3 screenMatrix,
        int viewportWidth,
        int viewportHeight,
        string? hoveredNodeId)
    {
        Draw(
            scene,
            camera,
            screenMatrix,
            viewportWidth,
            viewportHeight,
            hoveredNodeId,
            hoveredPod: null);
    }

    /// <summary>
    /// Draws visible labels and optional pod hover labels.
    /// </summary>
    public void Draw(
        ISceneSnapshot scene,
        CameraSnapshot camera,
        Matrix3 screenMatrix,
        int viewportWidth,
        int viewportHeight,
        string? hoveredNodeId,
        HoveredPodHud? hoveredPod)
    {
        if (scene.IsEmpty)
            return;

        IReadOnlyList<LabelVisibilityPlanner.PlannedLabel> labels =
            _planner.Plan(
                scene,
                camera,
                viewportWidth,
                viewportHeight,
                hoveredNodeId,
                hoveredPod);

        if (labels.Count == 0)
            return;

        textGpu.Begin();

        foreach (LabelVisibilityPlanner.PlannedLabel label in labels)
            DrawLabel(label);

        int glyphCount =
            textGpu.Upload();

        textGpu.Execute(
            textComputeShader,
            glyphCount,
            screenMatrix,
            new Vector2(
                viewportWidth,
                viewportHeight));

        textRenderer.DrawGpu(
            textGpu,
            textRenderShader,
            screenMatrix);
    }

    /// <summary>
    /// Draws a single planned label.
    /// </summary>
    private void DrawLabel(
        LabelVisibilityPlanner.PlannedLabel label)
    {
        float scale =
            GetScale(label);

        float maxWidth =
            GetMaxWidth(label);

        Vector4 color =
            GetColor(label);

        string text =
            textGpu.TruncateToWidth(
                label.Text,
                scale,
                maxWidth);

        if (string.IsNullOrWhiteSpace(text))
            return;

        Vector2 origin =
            new(
                label.ScreenPosition.X,
                label.ScreenPosition.Y);

        if (label.IsPod)
        {
            textGpu.DrawStringOutlined(
                text,
                origin,
                scale,
                color,
                ShadowColor,
                1.35f);

            return;
        }

        if (label.IsHovered)
        {
            textGpu.DrawStringOutlined(
                text,
                origin,
                scale,
                color,
                ShadowColor,
                1.25f);

            return;
        }

        if (label.IsPersistent)
        {
            textGpu.DrawStringOutlined(
                text,
                origin,
                scale,
                color,
                ShadowColor,
                1.0f);

            return;
        }

        textGpu.DrawStringShadowed(
            text,
            origin,
            scale,
            color,
            ShadowColor,
            new Vector2(
                1.3f,
                1.3f));
    }

    /// <summary>
    /// Resolves visual scale for a label.
    /// </summary>
    private static float GetScale(
        LabelVisibilityPlanner.PlannedLabel label)
    {
        if (label.IsPod)
            return PodLabelScale;

        if (label.IsHovered)
            return HoverLabelScale;

        return label.Kind switch
        {
            NodeKind.File => FileLabelScale,
            NodeKind.Root => FolderLabelScale * 1.12f,
            _ => FolderLabelScale
        };
    }

    /// <summary>
    /// Resolves maximum allowed width for a label.
    /// </summary>
    private static float GetMaxWidth(
        LabelVisibilityPlanner.PlannedLabel label)
    {
        if (label.IsPod)
            return MaxPodLabelWidth;

        if (label.IsHovered)
            return MaxHoverLabelWidth;

        return label.Kind switch
        {
            NodeKind.File => MaxFileLabelWidth,
            _ => MaxFolderLabelWidth
        };
    }

    /// <summary>
    /// Resolves label tint color.
    /// </summary>
    private static Vector4 GetColor(
        LabelVisibilityPlanner.PlannedLabel label)
    {
        if (label.IsPod)
            return PodColor;

        if (label.IsHovered)
            return HoverColor;

        return label.Kind switch
        {
            NodeKind.Root => RootColor,
            NodeKind.File => FileColor,
            _ => label.Score > 45000f ? LargeFolderColor : FolderColor
        };
    }
}