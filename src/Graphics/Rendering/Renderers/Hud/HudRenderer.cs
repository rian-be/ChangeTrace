using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Interfaces;
using ChangeTrace.Graphics.Rendering.Renderers.Text;
using ChangeTrace.Graphics.Shaders.Runtime;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Colors;
using ChangeTrace.Rendering.Hud;
using ChangeTrace.Rendering.States;
using OpenTK.Mathematics;
using GLVec4 = OpenTK.Mathematics.Vector4;
using Vector2 = OpenTK.Mathematics.Vector2;

namespace ChangeTrace.Graphics.Rendering.Renderers.Hud;

/// <summary>
/// Renders screen space HUD overlays using the GPU text pipeline.
/// </summary>
internal sealed class HudRenderer(
    TextGpuPipeline textGpu,
    TextRenderer textRenderer,
    ComputeShader textComputeShader,
    ShaderProgram textRenderShader) : IHudRenderer
{
    /// <summary>
    /// Draws the complete HUD layer.
    /// </summary>
    public void Draw(RenderState state, Matrix3 screenMatrix, float viewW, float viewH)
    {
        var white = new GLVec4( 0.95f, 0.95f, 0.95f, 0.9f);

        // Adaptive HUD scale based on viewport size.
        float hudScale = MathHelper.Clamp(
                Math.Min(
                    viewW / 1280f,
                    viewH / 720f),
                0.75f,
                1.5f);

        var layout = HudLayout.Create(viewW, viewH, hudScale);

        textGpu.Begin();

        DrawPlaybackHud(
            state.Hud.Playback,
            state.Camera.Zoom,
            white,
            layout);

        DrawContributorsList(
            state.Hud.Leaderboard,
            layout);

        DrawStatisticsHud(
            state.Hud.Statistics,
            state.Hud.Playback.LayoutMode,
            white,
            layout);

        DrawZoomSlider(
            state.Camera.Zoom,
            layout);

        DrawInteractionHud(
            state.Hud.Interaction,
            white,
            layout);

        int glyphCount =
            textGpu.Upload();

        // GPU glyph culling + compaction.
        textGpu.Execute(
            textComputeShader,
            glyphCount,
            screenMatrix,
            new Vector2(
                viewW,
                viewH));

        // GPU indirect glyph rendering.
        textRenderer.DrawGpu(
            textGpu,
            textRenderShader,
            screenMatrix);
    }

    /// <summary>
    /// Draws playback/date/zoom information at the top center.
    /// </summary>
    private void DrawPlaybackHud(PlaybackHud p, float zoom, GLVec4 color, HudLayout layout)
    {
        int currentDay =
            p.ElapsedDays + 1;

        string absoluteDate = p.AbsoluteDateLabel ?? "Fetching date...";

        string zoomStr = $"Zoom: {zoom:F2}x";
        string dateStr = $"Day {currentDay}  |  {absoluteDate}  |  {zoomStr}";

        float scale = 1.05f * layout.Scale;

        float charWidth = TextGpuPipeline.GlyphW * scale;
        dateStr = FitText(dateStr, layout.CenterWidth, charWidth);
        float width = dateStr.Length * charWidth;

        Vector2 pos = new(ClampX(
                layout.CenterX + (layout.CenterWidth - width) * 0.5f,
                layout
            ),
            layout.Top
        );

        textGpu.DrawString(
            dateStr,
            pos,
            scale,
            color);
    }

    /// <summary>
    /// Draws hovered node interaction information.
    /// </summary>
    private void DrawInteractionHud(InteractionHud i, GLVec4 color, HudLayout layout)
    {
        if (string.IsNullOrEmpty(i.HoveredNodeId))
            return;

        float hScale = 0.78f * layout.Scale;
        float lineSpace = 20f * layout.Scale;

        Vector2 hPos = new(layout.LeftX, layout.BottomInfoY);

        textGpu.DrawString(
            FitText(
                $"FILE:   {i.HoveredNodeId}",
                layout.BottomInfoWidth,
                TextGpuPipeline.GlyphW * hScale),
            hPos,
            hScale,
            color);

        if (!string.IsNullOrEmpty(i.HoveredNodeAuthor))
        {
            hPos.Y += lineSpace;

            textGpu.DrawString(
                FitText(
                    $"AUTHOR: {i.HoveredNodeAuthor}",
                    layout.BottomInfoWidth,
                    TextGpuPipeline.GlyphW * hScale),
                hPos,
                hScale,
                new GLVec4(0.4f, 0.8f, 1f, 0.9f));
        }

        if (!string.IsNullOrEmpty(i.HoveredNodeCommit))
        {
            hPos.Y += lineSpace;

            string shortSha =
                i.HoveredNodeCommit.Length > 8
                    ? i.HoveredNodeCommit[..8]
                    : i.HoveredNodeCommit;

            textGpu.DrawString(
                $"COMMIT: {shortSha}",
                hPos,
                hScale,
                new GLVec4(
                    0.7f,
                    0.7f,
                    0.7f,
                    0.8f));
        }
    }

    /// <summary>
    /// Draws repository statistics and layout mode information.
    /// </summary>
    private void DrawStatisticsHud(StatisticsHud s, LayoutMode mode, GLVec4 color, HudLayout layout)
    {
        float scale = 0.76f * layout.Scale;
        float x = layout.RightX;
        float y = layout.Top;

        textGpu.DrawString(
            $"Files: {s.TotalNodes}",
            new Vector2(
                x,
                y),
            scale,
            color);

        y += 21f * layout.Scale;

        var modeColor = mode == LayoutMode.Forest
                ? new GLVec4(0.4f, 1.0f, 0.5f, 0.9f)
                : new GLVec4(1.0f, 0.7f, 0.3f, 0.9f);

        textGpu.DrawString(
            FitText(
                $"[F] Mode: {mode}",
                layout.RightWidth,
                TextGpuPipeline.GlyphW * scale),
            new Vector2(
                x,
                y),
            scale,
            modeColor);

        y += 25f * layout.Scale;

        int maxRows = Math.Max(0,
                (int)((layout.ZoomTop - y - 18f * layout.Scale) / (19f * layout.Scale)));

        foreach (var group in s.Extensions.Take(maxRows))
        {
            var extColor = ColorPalette.ForFilePath("file" + group.Extension);
            var glColor = new GLVec4( extColor.X, extColor.Y, extColor.Z, extColor.W);

            textGpu.DrawString(
                FitText(
                    $"* {group.Extension}: {group.Count}",
                    layout.RightWidth,
                    TextGpuPipeline.GlyphW * scale),
                new Vector2(
                    x,
                    y),
                scale,
                glColor);

            y += 19f * layout.Scale;
        }
    }

    /// <summary>
    /// Draws the contributor leaderboard list.
    /// </summary>
    private void DrawContributorsList(IReadOnlyList<LeaderboardEntry> leaderboard, HudLayout layout)
    {
        if (leaderboard.Count == 0)
            return;

        float x = layout.LeftX;
        float y = layout.ListTop;

        float row = 27f * layout.Scale;
        float scale = 0.92f * layout.Scale;

        var titleColor = new GLVec4(1f, 1f, 1f, 0.9f);

        var nameColor = new GLVec4(0.9f, 0.9f, 0.9f, 0.8f);
        
        string title = $"Contributors  {leaderboard.Count}";

        textGpu.DrawString(
            FitText(
                title,
                layout.LeftWidth,
                TextGpuPipeline.GlyphW * scale),
            new Vector2(
                x,
                y),
            scale,
            titleColor);

        y += 33f * layout.Scale;
        int maxRows = Math.Max(0, (int)((layout.BottomInfoY - y - 16f * layout.Scale) / row));

        int showCount = Math.Min(
                Math.Min(
                    leaderboard.Count,
                    12),
                maxRows);

        for (int i = 0; i < showCount; i++)
        {
            var entry = leaderboard[i];
            var color = ColorPalette.ForActor(entry.Actor);
            var bulletColor = new GLVec4( color.X, color.Y, color.Z, 0.95f);

            textGpu.DrawString("*", new Vector2(
                    x,
                    y),
                scale,
                bulletColor);

            var label = FitText(
                    $"{entry.Actor} ({entry.EventCount})",
                    layout.LeftWidth - 22f * layout.Scale,
                    TextGpuPipeline.GlyphW * scale);

            textGpu.DrawString(
                label,
                new Vector2(
                    x + 22f * layout.Scale,
                    y),
                scale,
                nameColor);

            y += row;
        }
    }

    /// <summary>
    /// Draws the vertical logarithmic zoom slider.
    /// </summary>
    private void DrawZoomSlider(float zoom, HudLayout layout)
    {
        var labelColor = new GLVec4( 1f, 1f, 1f, 0.5f);
        var accent = new GLVec4(0.2f, 0.8f, 1.0f, 0.95f);

        float minZ = 0.15f;
        float maxZ = 10.0f;

        float logMin = (float)Math.Log(minZ);
        float logMax = (float)Math.Log(maxZ);

        float logCur = (float)Math.Log(
                MathHelper.Clamp(
                    zoom,
                    minZ,
                    maxZ));

        float t = (logCur - logMin) / (logMax - logMin);
        float x = layout.RightX + layout.RightWidth - 48f * layout.Scale;
        
        float yTop = layout.ZoomTop;
        float yBot = layout.ZoomBottom;
        float h = yBot - yTop;

        float yThumb = yBot - t * h;
        float scale = 0.74f * layout.Scale;

        textGpu.DrawString(
            "ZOOM",
            new Vector2(
                x - 28f * layout.Scale,
                yTop - 38f * layout.Scale),
            scale,
            labelColor);

        textGpu.DrawString("+", new Vector2(
                x - 6f * layout.Scale,
                yTop - 20f * layout.Scale),
            scale * 1.35f,
            accent);

        for (float ty = yTop; ty <= yBot; ty += 15f * layout.Scale)
        {
            bool selected = Math.Abs(ty - yThumb) < 8f * layout.Scale;

            textGpu.DrawString(selected ? "O" : "|",
                new Vector2(
                    x - 4f * layout.Scale,
                    ty),
                scale,
                selected ? accent : labelColor);
        }

        textGpu.DrawString("-", new Vector2(
                x - 5f * layout.Scale,
                yBot + 8f * layout.Scale
            ),
            scale * 1.35f,
            accent);

        string pct = $"{(int)(zoom * 100)}%";

        textGpu.DrawString(
            pct,
            new Vector2(
                x - 52f * layout.Scale,
                yThumb - 8f * layout.Scale
             ),
            scale,
            accent);
    }

    /// <summary>
    /// Truncates text to fit the available width.
    /// </summary>
    private static string FitText(string value, float maxWidth, float charWidth)
    {
        int maxChars = Math.Max(4, (int)(maxWidth / Math.Max(1f, charWidth)));
        if (value.Length <= maxChars)
            return value;

        return value[..Math.Max(1, maxChars - 3)] + "...";
    }

    /// <summary>
    /// Clamps HUD X coordinate into the visible viewport area.
    /// </summary>
    private static float ClampX(float x, HudLayout layout)
    {
        return MathHelper.Clamp(
            x,
            layout.Margin,
            Math.Max(
                layout.Margin,
                layout.ViewW - layout.Margin - 80f * layout.Scale
            )
        );
    }

    /// <summary>
    /// Precomputed responsive HUD layout values.
    /// </summary>
    private readonly record struct HudLayout(
        float ViewW,
        float ViewH,
        float Scale,
        float Margin,
        float Top,
        float LeftX,
        float LeftWidth,
        float ListTop,
        float CenterX,
        float CenterWidth,
        float RightX,
        float RightWidth,
        float ZoomTop,
        float ZoomBottom,
        float BottomInfoY,
        float BottomInfoWidth)
    {
        /// <summary>
        /// Creates a responsive HUD layout for the current viewport.
        /// </summary>
        internal static HudLayout Create(
            float viewW,
            float viewH,
            float scale)
        {
            float margin = 16f * scale;
            
            float leftWidth = Math.Min(
                300f * scale,
                Math.Max(
                    180f * scale,
                    viewW * 0.28f
                )
            );

            float rightWidth = Math.Min(
                230f * scale,
                Math.Max(
                    150f * scale,
                    viewW * 0.22f
                )
            );

            float leftX = margin;
            float rightX = Math.Max(margin, viewW - margin - rightWidth);
            float centerX = leftX + leftWidth + margin;

            float centerWidth = Math.Max(
                120f * scale,
                rightX - margin - centerX
           );

            float bottomInfoY = Math.Max(
                120f * scale,
                viewH - 92f * scale
            );

            return new HudLayout(
                viewW,
                viewH,
                scale,
                margin,
                24f * scale,
                leftX,
                leftWidth,
                88f * scale,
                centerX,
                centerWidth,
                rightX,
                rightWidth,
                Math.Max(150f * scale, viewH * 0.48f),
                Math.Min(viewH - 80f * scale, viewH * 0.78f),
                bottomInfoY,
                Math.Max(180f * scale, viewW - margin * 2f));
        }
    }
}