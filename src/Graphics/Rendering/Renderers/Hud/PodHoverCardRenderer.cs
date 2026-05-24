using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Renderers.Sprites;
using ChangeTrace.Graphics.Shaders.Registry;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.States;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Hud;

/// <summary>
/// Renders interactive hover cards for heavy folder pods.
/// </summary>
internal sealed class PodHoverCardRenderer : IDisposable
{
    /// <summary>
    /// Hover card layout dimensions.
    /// </summary>
    private const float CardWidth = 330f;
    private const float CardHeight = 168f;
    private const float GlowPadding = 36f;

    /// <summary>
    /// Text rendering scales.
    /// </summary>
    private const float TitleScale = 1.08f;
    private const float BodyScale = 0.78f;
    private const float HintScale = 0.70f;
    private const float BadgeScale = 0.78f;

    /// <summary>
    /// Badge layout configuration.
    /// </summary>
    private const float BadgeWidth = 86f;
    private const float BadgeHeight = 32f;
    private const float BadgeOffsetRight = 24f;
    private const float BadgeOffsetTop = 20f;

    /// <summary>
    /// Language chip layout configuration.
    /// </summary>
    private const float ChipWidth = 90f;
    private const float ChipHeight = 32f;
    private const float ChipGap = 12f;
    private const float ChipIconSize = 16f;

    /// <summary>
    /// Unit quad vertices.
    /// </summary>
    private static readonly float[] QuadVertices =
    [
        0f, 0f,
        1f, 0f,
        1f, 1f,

        0f, 0f,
        1f, 1f,
        0f, 1f
    ];

    /// <summary>
    /// Hover card text colors.
    /// </summary>
    private static readonly Vector4 TitleColor =
        new(0.90f, 0.95f, 1.00f, 1.00f);

    private static readonly Vector4 BodyColor =
        new(0.66f, 0.72f, 0.86f, 0.94f);

    private static readonly Vector4 MutedColor =
        new(0.43f, 0.48f, 0.64f, 0.78f);

    private static readonly Vector4 ActivityColor =
        new(0.70f, 0.80f, 1.00f, 1.00f);

    private static readonly Vector4 CppColor =
        new(0.30f, 0.40f, 1.00f, 1.00f);

    private static readonly Vector4 HeaderColor =
        new(0.72f, 0.32f, 0.88f, 1.00f);

    private static readonly Vector4 MarkdownColor =
        new(0.42f, 0.82f, 1.00f, 1.00f);

    private static readonly Vector4 ShadowColor =
        new(0.00f, 0.00f, 0.00f, 0.95f);

    private static readonly Vector4 IconColor =
        new(0.96f, 0.98f, 1.00f, 0.98f);

    private readonly TextGpuPipeline _textPipeline;
    private readonly IconSpriteRenderer? _iconRenderer;

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _quadVbo = new();

    private bool _ready;

    private readonly record struct ChipUi(
        string Label,
        string Value,
        LanguageIcon Icon,
        Vector4 Color);

    internal PodHoverCardRenderer(
        TextGpuPipeline textPipeline,
        IconSpriteRenderer? iconRenderer)
    {
        _textPipeline = textPipeline;
        _iconRenderer = iconRenderer;

        InitializeGpu();
    }

    /// <summary>
    /// Draws the active hover card HUD.
    /// </summary>
    internal void Draw(
        RenderResources resources,
        RenderState state,
        Matrix3 viewProj,
        int viewportWidth,
        int viewportHeight,
        float time)
    {
        if (!_ready)
            return;

        HoveredPodHud? pod =
            state.Hud.Interaction.HoveredPod;

        if (pod == null)
            return;

        Vector2 podScreen =
            ProjectWorldToScreen(
                pod.Center,
                viewProj,
                viewportWidth,
                viewportHeight);

        Vec2 cardWorld =
            GetCardWorldPosition(pod);

        Vector2 cardScreen =
            ProjectWorldToScreen(
                cardWorld,
                viewProj,
                viewportWidth,
                viewportHeight);

        if (!IsScreenPointNearViewport(
                cardScreen,
                viewportWidth,
                viewportHeight,
                220f))
        {
            return;
        }

        Vector4 panelRect =
            new(
                cardScreen.X - CardWidth * 0.5f,
                cardScreen.Y - CardHeight * 0.5f,
                CardWidth,
                CardHeight);

        Vector4 drawRect =
            new(
                panelRect.X - GlowPadding,
                panelRect.Y - GlowPadding,
                panelRect.Z + GlowPadding * 2f,
                panelRect.W + GlowPadding * 2f + 22f);

        Vector4 badgeRect =
            GetBadgeRect(panelRect);

        Vector4 chip0Rect =
            GetChipRect(panelRect, 0);

        Vector4 chip1Rect =
            GetChipRect(panelRect, 1);

        Vector4 chip2Rect =
            GetChipRect(panelRect, 2);

        DrawPanel(
            resources,
            panelRect,
            drawRect,
            badgeRect,
            chip0Rect,
            chip1Rect,
            chip2Rect,
            podScreen,
            new Vector2(
                viewportWidth,
                viewportHeight),
            time);

        DrawText(
            resources,
            pod,
            panelRect,
            new Vector2(
                viewportWidth,
                viewportHeight));
    }

    /// <summary>
    /// Initializes GPU buffers for panel rendering.
    /// </summary>
    private void InitializeGpu()
    {
        _vao.Bind();

        _quadVbo.Upload(
            QuadVertices,
            BufferUsageHint.StaticDraw);

        _vao.BindVertexBuffer(_quadVbo);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: 2 * sizeof(float),
            offsetBytes: 0);

        VertexArray.Unbind();

        _ready = true;
    }

    /// <summary>
    /// Draws the hover card panel background.
    /// </summary>
    private void DrawPanel(
        RenderResources resources,
        Vector4 panelRect,
        Vector4 drawRect,
        Vector4 badgeRect,
        Vector4 chip0Rect,
        Vector4 chip1Rect,
        Vector4 chip2Rect,
        Vector2 podCenterPx,
        Vector2 viewport,
        float time)
    {
        GL.Enable(EnableCap.Blend);

        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);

        ShaderProgram shader =
            resources.Shaders.Graphics(
                "PodHoverCard");

        shader.Use();

        shader.Set("uRectPx", drawRect);
        shader.Set("uPanelPx", panelRect);
        shader.Set("uBadgePx", badgeRect);
        shader.Set("uChip0Px", chip0Rect);
        shader.Set("uChip1Px", chip1Rect);
        shader.Set("uChip2Px", chip2Rect);
        shader.Set("uPodCenterPx", podCenterPx);
        shader.Set("uViewport", viewport);
        shader.Set("uTime", time);
        shader.Set("uRadius", 18.0f);
        shader.Set("uBorderWidth", 1.35f);
        shader.Set("uGlowSize", 34.0f);

        _vao.Bind();

        _vao.DrawArrays(
            PrimitiveType.Triangles,
            vertexCount: 6);

        VertexArray.Unbind();
    }

    /// <summary>
    /// Draws hover card text and icon content.
    /// </summary>
    private void DrawText(
        RenderResources resources,
        HoveredPodHud pod,
        Vector4 panelRect,
        Vector2 viewport)
    {
        string title =
            FormatTitle(pod);

        int fileCount =
            Math.Max(
                0,
                pod.FileIds.Count);

        float left =
            panelRect.X + 24f;

        float top =
            panelRect.Y + 24f;

        Vector4 badgeRect =
            GetBadgeRect(panelRect);

        _textPipeline.Begin();

        DrawLanguageIcon(
            resources,
            LanguageIcon.CplusplusPlain,
            new Vector4(
                left,
                top + 2f,
                18f,
                18f),
            viewport,
            IconColor);

        string safeTitle =
            _textPipeline.TruncateToWidth(
                title,
                TitleScale,
                162f);

        float titleIconY =
            top + 2f;

        float titleIconSize =
            18f;

        float titleTextHeight =
            _textPipeline.MeasureHeight(
                TitleScale);

        float titleTextY =
            titleIconY +
            (titleIconSize - titleTextHeight) * 0.5f - 1f;

        _textPipeline.DrawStringOutlined(
            safeTitle,
            new Vector2(
                left + 30f,
                titleTextY),
            TitleScale,
            TitleColor,
            ShadowColor,
            0.85f);

        DrawBadge(
            fileCount,
            badgeRect);

        DrawChip(
            resources,
            new ChipUi(
                "C++",
                "42%",
                LanguageIcon.CplusplusPlain,
                CppColor),
            GetChipRect(panelRect, 0),
            viewport);

        DrawChip(
            resources,
            new ChipUi(
                "C#",
                "12%",
                LanguageIcon.CsharpPlain,
                HeaderColor),
            GetChipRect(panelRect, 1),
            viewport);

        DrawChip(
            resources,
            new ChipUi(
                "MD",
                "2%",
                LanguageIcon.CsharpPlain,
                MarkdownColor),
            GetChipRect(panelRect, 2),
            viewport);

        float activityY =
            top + 102f;

        _textPipeline.DrawStringShadowed(
            "Activity",
            new Vector2(left, activityY),
            BodyScale,
            BodyColor,
            ShadowColor,
            new Vector2(1.1f, 1.1f));

        _textPipeline.DrawStringShadowed(
            "▁▂▃▄▅▆▇█",
            new Vector2(
                left + 86f,
                activityY),
            BodyScale,
            ActivityColor,
            ShadowColor,
            new Vector2(1.1f, 1.1f));

        _textPipeline.DrawStringShadowed(
            GetActivityLabel(pod),
            new Vector2(
                panelRect.X + panelRect.Z - 58f,
                activityY),
            BodyScale,
            ActivityColor,
            ShadowColor,
            new Vector2(1.1f, 1.1f));

        _textPipeline.DrawStringShadowed(
            "Click to focus",
            new Vector2(
                left,
                top + 132f),
            HintScale,
            MutedColor,
            ShadowColor,
            new Vector2(1.0f, 1.0f));

        int glyphCount =
            _textPipeline.Upload();

        Matrix3 screenMatrix =
            BuildScreenMatrix(
                viewport.X,
                viewport.Y);

        _textPipeline.Execute(
            resources.Shaders.Compute(
                "TextCull"),
            glyphCount,
            screenMatrix,
            viewport);

        resources.Renderers.Text.DrawGpu(
            _textPipeline,
            resources.Shaders.Graphics(
                "Text"),
            screenMatrix);
    }

    /// <summary>
    /// Draws file count badge.
    /// </summary>
    private void DrawBadge(
        int fileCount,
        Vector4 badgeRect)
    {
        string badge =
            $"{fileCount} files";

        float badgeTextWidth =
            _textPipeline.MeasureWidth(
                badge,
                BadgeScale);

        float badgeTextHeight =
            _textPipeline.MeasureHeight(
                BadgeScale);

        Vector2 badgePos =
            new(
                badgeRect.X + (badgeRect.Z - badgeTextWidth) * 0.5f,
                badgeRect.Y + (badgeRect.W - badgeTextHeight) * 0.5f - 0.5f);

        _textPipeline.DrawStringOutlined(
            badge,
            badgePos,
            BadgeScale,
            TitleColor,
            ShadowColor,
            0.75f);
    }

    /// <summary>
    /// Draws a language statistics chip.
    /// </summary>
    private void DrawChip(
        RenderResources resources,
        ChipUi chip,
        Vector4 chipRect,
        Vector2 viewport)
    {
        float iconX =
            chipRect.X + 10f;

        float iconY =
            chipRect.Y + (ChipHeight - ChipIconSize) * 0.5f;

        DrawLanguageIcon(
            resources,
            chip.Icon,
            new Vector4(
                iconX,
                iconY,
                ChipIconSize,
                ChipIconSize),
            viewport,
            IconColor);

        float labelX =
            chipRect.X + 32f;

        float labelY =
            chipRect.Y + 9f;

        _textPipeline.DrawStringShadowed(
            chip.Label,
            new Vector2(labelX, labelY),
            BodyScale,
            chip.Color,
            ShadowColor,
            new Vector2(1.0f, 1.0f));

        _textPipeline.DrawStringShadowed(
            chip.Value,
            new Vector2(
                chipRect.X + 60f,
                labelY),
            BodyScale,
            BodyColor,
            ShadowColor,
            new Vector2(1.0f, 1.0f));
    }

    /// <summary>
    /// Draws a language icon sprite.
    /// </summary>
    private void DrawLanguageIcon(
        RenderResources resources,
        LanguageIcon icon,
        Vector4 rectPx,
        Vector2 viewport,
        Vector4 tint)
    {
        if (_iconRenderer == null)
            return;

        _iconRenderer.Draw(
            resources.Shaders.Graphics(
                "IconSprite"),
            icon,
            rectPx,
            viewport,
            tint);
    }

    /// <summary>
    /// Builds orthographic screen projection matrix.
    /// </summary>
    private static Matrix3 BuildScreenMatrix(
        float viewportWidth,
        float viewportHeight)
    {
        Matrix3 m =
            Matrix3.Identity;

        m.M11 =
            2f / viewportWidth;

        m.M22 =
            -2f / viewportHeight;

        m.M31 =
            -1f;

        m.M32 =
            1f;

        return m;
    }

    /// <summary>
    /// Resolves badge rectangle inside the panel.
    /// </summary>
    private static Vector4 GetBadgeRect(
        Vector4 panelRect)
    {
        return new Vector4(
            panelRect.X + panelRect.Z - BadgeOffsetRight - BadgeWidth,
            panelRect.Y + BadgeOffsetTop,
            BadgeWidth,
            BadgeHeight);
    }

    /// <summary>
    /// Resolves chip rectangle inside the panel.
    /// </summary>
    private static Vector4 GetChipRect(
        Vector4 panelRect,
        int index)
    {
        float left =
            panelRect.X + 24f;

        float top =
            panelRect.Y + 66f;

        return new Vector4(
            left + index * (ChipWidth + ChipGap),
            top,
            ChipWidth,
            ChipHeight);
    }

    /// <summary>
    /// Formats hover card title text.
    /// </summary>
    private static string FormatTitle(
        HoveredPodHud pod)
    {
        string[] parts =
            pod.Label.Split(
                '·',
                StringSplitOptions.TrimEntries |
                StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 0)
            return pod.Label;

        return parts[0];
    }

    /// <summary>
    /// Resolves activity label text.
    /// </summary>
    private static string GetActivityLabel(
        HoveredPodHud pod)
    {
        return pod.ActivityScore switch
        {
            >= 0.65f => "High",
            >= 0.25f => "Medium",
            _ => "Low"
        };
    }

    /// <summary>
    /// Projects world position into screen space.
    /// </summary>
    private static Vector2 ProjectWorldToScreen(
        Vec2 worldPosition,
        Matrix3 viewProj,
        int viewportWidth,
        int viewportHeight)
    {
        float ndcX =
            viewProj.M11 * worldPosition.X +
            viewProj.M21 * worldPosition.Y +
            viewProj.M31;

        float ndcY =
            viewProj.M12 * worldPosition.X +
            viewProj.M22 * worldPosition.Y +
            viewProj.M32;

        return new Vector2(
            (ndcX + 1f) * 0.5f * viewportWidth,
            (1f - ndcY) * 0.5f * viewportHeight);
    }

    /// <summary>
    /// Checks whether a point is near the viewport.
    /// </summary>
    private static bool IsScreenPointNearViewport(
        Vector2 point,
        int viewportWidth,
        int viewportHeight,
        float padding)
    {
        return
            point.X >= -padding &&
            point.Y >= -padding &&
            point.X <= viewportWidth + padding &&
            point.Y <= viewportHeight + padding;
    }

    /// <summary>
    /// Resolves hover card world position above the pod.
    /// </summary>
    private static Vec2 GetCardWorldPosition(
        HoveredPodHud pod)
    {
        return
            pod.Center +
            new Vec2(
                0f,
                -MathF.Min(
                    pod.Radius * 0.34f,
                    92f));
    }

    public void Dispose()
    {
        if (!_ready)
            return;

        _quadVbo.Dispose();
        _vao.Dispose();

        _ready = false;
    }
}