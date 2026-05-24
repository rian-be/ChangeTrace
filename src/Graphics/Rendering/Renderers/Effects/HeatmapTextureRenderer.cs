using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Rendering.Renderers.Effects;

/// <summary>
/// Renders a fullscreen heatmap texture overlay.
/// </summary>
internal sealed class HeatmapTextureRenderer : IDisposable
{
    /// <summary>
    /// Fullscreen quad vertices with texture coordinates.
    /// </summary>
    private static readonly float[] Quad =
    [
        -1f, -1f, 0f, 0f,
         1f, -1f, 1f, 0f,
        -1f,  1f, 0f, 1f,

         1f, -1f, 1f, 0f,
         1f,  1f, 1f, 1f,
        -1f,  1f, 0f, 1f
    ];

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _quadVbo = new();

    private bool _disposed;

    public HeatmapTextureRenderer()
    {
        InitializeGpu();
    }

    /// <summary>
    /// Draws the heatmap texture using grading shader parameters.
    /// </summary>
    public void Draw(
        int texture,
        ShaderProgram shader)
    {
        if (_disposed)
            return;

        shader.Use();

        GL.ActiveTexture(TextureUnit.Texture0);
        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        shader.Set("uTexture", 0);
        shader.Set("uOpacity", 0.85f);
        shader.Set("uIntensity", 1.35f);
        shader.Set("uGamma", 0.95f);
        shader.Set("uContrast", 1.18f);
        shader.Set("uSaturation", 1.35f);
        shader.Set("uUseSoftUpscale", 1);

        _vao.Bind();

        _vao.DrawArrays(
            PrimitiveType.Triangles,
            vertexCount: 6);

        VertexArray.Unbind();

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);
    }

    /// <summary>
    /// Initializes fullscreen quad GPU buffers.
    /// </summary>
    private void InitializeGpu()
    {
        _vao.Bind();

        _quadVbo.Upload(
            Quad,
            BufferUsageHint.StaticDraw);

        _vao.BindVertexBuffer(_quadVbo);

        int stride =
            4 * sizeof(float);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: stride,
            offsetBytes: 0);

        _vao.AttributePointer(
            index: 1,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: stride,
            offsetBytes: 2 * sizeof(float));

        VertexArray.Unbind();
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _quadVbo.Dispose();
        _vao.Dispose();

        _disposed = true;
    }
}