using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Gpu.Pipelines;
using ChangeTrace.Graphics.Rendering.Text;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Rendering.Renderers.Text;

/// <summary>
/// Renders GPU driven bitmap glyph instances.
/// Uses monospace atlas texture.
/// </summary>
internal sealed class TextRenderer : IDisposable
{
    /// <summary>
    /// Unit quad vertices used per glyph instance.
    /// </summary>
    private static readonly float[] QuadVerts =
    [
        0f, 1f,
        1f, 1f,
        0f, 0f,

        1f, 1f,
        1f, 0f,
        0f, 0f
    ];

    private readonly int _atlasTexture;

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _quadVbo = new();

    internal TextRenderer(string atlasPath)
    {
        _atlasTexture =
            CreateAtlasTexture(atlasPath);

        CreateVertexArray();
    }

    /// <summary>
    /// Draws visible glyphs using GPU-generated indirect commands.
    /// </summary>
    internal void DrawGpu(
        TextGpuPipeline gpu,
        ShaderProgram renderShader,
        Matrix3 viewProj)
    {
        if (gpu.GlyphCount <= 0)
            return;

        renderShader.Use();

        renderShader.Set(
            "uViewProj",
            viewProj);

        renderShader.Set(
            "uAtlas",
            0);

        renderShader.Set(
            "uAtlasSize",
            new Vector2(
                TextGpuPipeline.AtlasW,
                TextGpuPipeline.AtlasH));

        renderShader.Set(
            "uGlyphSize",
            new Vector2(
                TextGpuPipeline.GlyphW,
                TextGpuPipeline.GlyphH));

        renderShader.Set(
            "uCharsPerRow",
            TextGpuPipeline.AtlasW / TextGpuPipeline.GlyphW);

        GL.ActiveTexture(
            TextureUnit.Texture0);

        GL.BindTexture(
            TextureTarget.Texture2D,
            _atlasTexture);

        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            1,
            gpu.VisibleGlyphBuffer);

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            gpu.IndirectBuffer);

        _vao.Bind();

        _vao.DrawArraysIndirect(
            PrimitiveType.Triangles);

        VertexArray.Unbind();

        GL.BindBuffer(
            BufferTarget.DrawIndirectBuffer,
            0);

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);
    }

    /// <summary>
    /// Creates glyph atlas texture storage.
    /// </summary>
    private static int CreateAtlasTexture(string atlasPath)
    {
        int texture =
            GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMinFilter,
            (int)TextureMinFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureMagFilter,
            (int)TextureMagFilter.Linear);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapS,
            (int)TextureWrapMode.ClampToEdge);

        GL.TexParameter(
            TextureTarget.Texture2D,
            TextureParameterName.TextureWrapT,
            (int)TextureWrapMode.ClampToEdge);

        byte[] pixels;
        int width;
        int height;

        (pixels, width, height) =
            FontAtlasLoader.LoadOrCreate(
                atlasPath,
                TextGpuPipeline.AtlasW,
                TextGpuPipeline.AtlasH,
                TextGpuPipeline.GlyphW,
                TextGpuPipeline.GlyphH);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.R8,
            width,
            height,
            0,
            PixelFormat.Red,
            PixelType.UnsignedByte,
            pixels);

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        return texture;
    }

    /// <summary>
    /// Initializes GPU buffers for glyph rendering.
    /// </summary>
    private void CreateVertexArray()
    {
        _vao.Bind();

        _vao.BindVertexBuffer(_quadVbo);

        _quadVbo.Upload(
            QuadVerts,
            BufferUsageHint.StaticDraw);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: 2 * sizeof(float),
            offsetBytes: 0);

        VertexArray.Unbind();
    }

    public void Dispose()
    {
        GL.DeleteTexture(_atlasTexture);

        _quadVbo.Dispose();
        _vao.Dispose();
    }
}