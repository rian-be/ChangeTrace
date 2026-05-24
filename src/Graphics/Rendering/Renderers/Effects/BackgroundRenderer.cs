using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Rendering.Renderers.Effects;

/// <summary>
/// Renders the fullscreen background gradient.
/// </summary>
internal sealed class BackgroundRenderer : IDisposable
{
    /// <summary>
    /// Fullscreen triangle quad vertices.
    /// </summary>
    private static readonly float[] BackgroundQuad =
    [
        -1f, -1f,
        1f, -1f,
        -1f,  1f,

        1f, -1f,
        1f,  1f,
        -1f,  1f
    ];

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _vbo = new();

    public BackgroundRenderer()
    {
        _vao.Bind();

        _vbo.Upload(
            BackgroundQuad,
            BufferUsageHint.StaticDraw);

        GL.EnableVertexAttribArray(0);

        GL.VertexAttribPointer(
            0,
            2,
            VertexAttribPointerType.Float,
            false,
            2 * sizeof(float),
            0);

        VertexArray.Unbind();
    }

    /// <summary>
    /// Draws the background gradient.
    /// </summary>
    public void Draw(ShaderProgram shader)
    {
        GL.Disable(EnableCap.Blend);

        shader.Use();

        _vao.Bind();

        GL.DrawArrays(
            PrimitiveType.Triangles,
            0,
            6);

        VertexArray.Unbind();

        GL.Enable(EnableCap.Blend);
    }

    public void Dispose()
    {
        _vbo.Dispose();
        _vao.Dispose();
    }
}