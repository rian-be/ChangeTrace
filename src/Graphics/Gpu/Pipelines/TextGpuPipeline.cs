using ChangeTrace.Graphics.Gpu.Contracts;
using ChangeTrace.Graphics.Gpu.Pipelines.Base;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using GpuVector2 = System.Numerics.Vector2;
using GpuVector4 = System.Numerics.Vector4;

namespace ChangeTrace.Graphics.Gpu.Pipelines;

/// <summary>
/// GPU pipeline for bitmap glyph rendering.
/// Handles glyph generation, visibility compaction and indirect rendering.
/// </summary>
internal sealed class TextGpuPipeline
    : IndirectComputePipeline<GpuGlyph>
{
    internal const int AtlasW = 128;
    internal const int AtlasH = 84;

    internal const int GlyphW = 8;
    internal const int GlyphH = 14;

    /// <summary>
    /// Supported glyph character range.
    /// </summary>
    private const int FirstChar = 32;
    private const int LastCharExclusive = 128;

    /// <summary>
    /// Total number of supported glyphs.
    /// </summary>
    private const int SupportedCharCount =
        LastCharExclusive - FirstChar;

    /// <summary>
    /// GPU glyph upload capacity.
    /// </summary>
    private const int MaxGlyphs = 32768;

    private readonly GpuGlyph[] _cpuUpload =
        new GpuGlyph[MaxGlyphs];

    private int _glyphCount;

    /// <summary>
    /// Visible compacted glyph buffer handle.
    /// </summary>
    public int VisibleGlyphBuffer =>
        VisibleStorageBuffer.Handle;

    /// <summary>
    /// Indirect draw command buffer handle.
    /// </summary>
    public int IndirectBuffer =>
        IndirectDrawBuffer.Handle;

    /// <summary>
    /// Source glyph buffer handle.
    /// </summary>
    public int InputGlyphBuffer =>
        InputStorageBuffer.Handle;

    /// <summary>
    /// Current uploaded glyph count.
    /// </summary>
    public int GlyphCount =>
        _glyphCount;

    public TextGpuPipeline()
    {
        Initialize(
            maxItems: MaxGlyphs,
            vertexCountPerInstance: 6,
            inputUsage: BufferUsageHint.StreamDraw);
    }

    /// <summary>
    /// Begins a new glyph batch.
    /// </summary>
    internal void Begin()
    {
        _glyphCount = 0;
    }

    /// <summary>
    /// Uploads generated glyphs into GPU storage.
    /// </summary>
    internal int Upload()
    {
        if (_glyphCount <= 0)
            return 0;

        InputStorageBuffer.SubData(
            _cpuUpload.AsSpan(
                0,
                _glyphCount));

        return _glyphCount;
    }

    /// <summary>
    /// Executes compute visibility pass for uploaded glyphs.
    /// </summary>
    internal void Execute(
        ComputeShader computeShader,
        int glyphCount,
        Matrix3 viewProj,
        Vector2 viewport)
    {
        if (glyphCount <= 0)
            return;

        ResetIndirect();

        computeShader.Use();
        computeShader.Set("uGlyphCount", glyphCount);
        computeShader.Set("uViewProj", viewProj);
        computeShader.Set("uViewport", viewport);

        BindCommonBuffers();
        Dispatch256(glyphCount);
    }

    /// <summary>
    /// Measures rendered string width in pixels.
    /// </summary>
    internal float MeasureWidth(
        string? text,
        float scale)
    {
        if (string.IsNullOrEmpty(text))
            return 0f;

        return text.Length * GlyphW * scale;
    }

    /// <summary>
    /// Measures rendered glyph height in pixels.
    /// </summary>
    internal float MeasureHeight(float scale)
    {
        return GlyphH * scale;
    }

    /// <summary>
    /// Truncates text to fit within a maximum width.
    /// </summary>
    internal string TruncateToWidth(
        string text,
        float scale,
        float maxWidth)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;

        if (maxWidth <= 0f)
            return string.Empty;

        float fullWidth =
            MeasureWidth(
                text,
                scale);

        if (fullWidth <= maxWidth)
            return text;

        const string ellipsis = "...";

        float ellipsisWidth =
            MeasureWidth(
                ellipsis,
                scale);

        if (ellipsisWidth >= maxWidth)
            return string.Empty;

        int maxChars =
            Math.Max(
                0,
                (int)((maxWidth - ellipsisWidth) /
                    (GlyphW * scale)));

        if (maxChars <= 0)
            return ellipsis;

        if (maxChars >= text.Length)
            return text;

        return text[..maxChars] + ellipsis;
    }

    /// <summary>
    /// Draws plain text glyphs.
    /// </summary>
    internal void DrawString(
        string text,
        Vector2 origin,
        float scale,
        Vector4 color,
        bool flipY = false)
    {
        DrawStringWithKind(
            text,
            origin,
            scale,
            color,
            flipY,
            kind: 0);
    }

    /// <summary>
    /// Draws shadowed text glyphs.
    /// </summary>
    internal void DrawStringShadowed(
        string text,
        Vector2 origin,
        float scale,
        Vector4 color,
        Vector4 shadowColor,
        Vector2 shadowOffset,
        bool flipY = false)
    {
        if (string.IsNullOrEmpty(text))
            return;

        DrawStringWithKind(
            text,
            origin + shadowOffset,
            scale,
            shadowColor,
            flipY,
            kind: 1);

        DrawStringWithKind(
            text,
            origin,
            scale,
            color,
            flipY,
            kind: 0);
    }

    /// <summary>
    /// Draws outlined text glyphs.
    /// </summary>
    internal void DrawStringOutlined(
        string text,
        Vector2 origin,
        float scale,
        Vector4 color,
        Vector4 outlineColor,
        float outlineOffset,
        bool flipY = false)
    {
        if (string.IsNullOrEmpty(text))
            return;

        DrawStringWithKind(
            text,
            origin + new Vector2(-outlineOffset, 0f),
            scale,
            outlineColor,
            flipY,
            kind: 2);

        DrawStringWithKind(
            text,
            origin + new Vector2(outlineOffset, 0f),
            scale,
            outlineColor,
            flipY,
            kind: 2);

        DrawStringWithKind(
            text,
            origin + new Vector2(0f, -outlineOffset),
            scale,
            outlineColor,
            flipY,
            kind: 2);

        DrawStringWithKind(
            text,
            origin + new Vector2(0f, outlineOffset),
            scale,
            outlineColor,
            flipY,
            kind: 2);

        DrawStringWithKind(
            text,
            origin,
            scale,
            color,
            flipY,
            kind: 0);
    }

    /// <summary>
    /// Generates glyph instances for a string.
    /// </summary>
    private void DrawStringWithKind(
        string text,
        Vector2 origin,
        float scale,
        Vector4 color,
        bool flipY,
        int kind)
    {
        if (string.IsNullOrEmpty(text))
            return;

        for (int i = 0; i < text.Length; i++)
        {
            if (_glyphCount >= MaxGlyphs)
                return;

            WriteGlyph(
                text[i],
                origin,
                i,
                scale,
                color,
                flipY,
                kind);
        }
    }

    /// <summary>
    /// Writes a single glyph into the upload buffer.
    /// </summary>
    private void WriteGlyph(
        char rawChar,
        Vector2 origin,
        int index,
        float scale,
        Vector4 color,
        bool flipY,
        int kind)
    {
        int glyphIndex =
            Math.Clamp(
                rawChar - FirstChar,
                0,
                SupportedCharCount - 1);

        float x =
            origin.X +
            index * GlyphW * scale;

        float y =
            flipY
                ? origin.Y - GlyphH * scale
                : origin.Y;

        _cpuUpload[_glyphCount++] = new GpuGlyph
        {
            Position = new GpuVector2(
                x,
                y),
            Size = new GpuVector2(
                GlyphW * scale,
                GlyphH * scale),
            Color = new GpuVector4(
                color.X,
                color.Y,
                color.Z,
                color.W),
            GlyphIndex = glyphIndex,
            FlipY = flipY ? 1 : 0,
            Kind = kind,
            Pad0 = 0
        };
    }
}