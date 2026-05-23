using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Rendering.Passes;

/// <summary>
/// Renders a fullscreen trail accumulation texture.
/// Owns the render target, but not the shader.
/// </summary>
internal sealed class TrailAccumulationPass : IDisposable
{
    /// <summary>
    /// Fullscreen triangle-strip quad vertices.
    /// </summary>
    private static readonly float[] TrailQuad =
    [
        -1f, -1f,
         1f, -1f,
        -1f,  1f,
         1f,  1f
    ];

    private readonly int _trailTexture;
    private readonly int _trailFbo;

    private readonly VertexArray _trailVao = new();
    private readonly VertexBuffer<float> _trailVbo = new();

    private readonly ShaderProgram _trailShader;

    private readonly int _width;
    private readonly int _height;

    private float _time;

    /// <summary>
    /// Trail accumulation texture handle.
    /// </summary>
    public int Texture =>
        _trailTexture;

    public TrailAccumulationPass(
        int width,
        int height,
        ShaderProgram trailShader)
    {
        _width = Math.Max(1, width);
        _height = Math.Max(1, height);

        _trailShader = trailShader;

        _trailTexture = CreateTrailTexture(
            _width,
            _height);

        _trailFbo = CreateFramebuffer(
            _trailTexture);

        InitializeFullscreenQuad();
    }

    /// <summary>
    /// Advances trail animation time.
    /// </summary>
    public void Update(float dt) =>
        _time += dt;

    /// <summary>
    /// Renders current trail state into the accumulation texture.
    /// </summary>
    public void Render()
    {
        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            _trailFbo);

        GL.Viewport(
            0,
            0,
            _width,
            _height);

        GL.Clear(ClearBufferMask.ColorBufferBit);

        _trailShader.Use();
        _trailShader.Set(
            "time",
            _time);

        _trailVao.Bind();

        GL.DrawArrays(
            PrimitiveType.TriangleStrip,
            0,
            4);

        VertexArray.Unbind();

        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            0);
    }

    /// <summary>
    /// Initializes fullscreen quad GPU buffers.
    /// </summary>
    private void InitializeFullscreenQuad()
    {
        _trailVao.Bind();

        _trailVbo.Upload(
            TrailQuad,
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
    /// Creates trail accumulation texture storage.
    /// </summary>
    private static int CreateTrailTexture(
        int width,
        int height)
    {
        int texture = GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba8,
            width,
            height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            IntPtr.Zero);

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

        GL.BindTexture(TextureTarget.Texture2D, 0);
        return texture;
    }

    /// <summary>
    /// Creates a framebuffer targeting the trail texture.
    /// </summary>
    private static int CreateFramebuffer(int texture)
    {
        int framebuffer =
            GL.GenFramebuffer();

        GL.BindFramebuffer(
            FramebufferTarget.Framebuffer,
            framebuffer);

        GL.FramebufferTexture2D(
            FramebufferTarget.Framebuffer,
            FramebufferAttachment.ColorAttachment0,
            TextureTarget.Texture2D,
            texture,
            0);

        GL.BindFramebuffer(FramebufferTarget.Framebuffer, 0);
        return framebuffer;
    }

    public void Dispose()
    {
        _trailVbo.Dispose();
        _trailVao.Dispose();

        GL.DeleteTexture(_trailTexture);
        GL.DeleteFramebuffer(_trailFbo);

        // Shader is owned by ShaderResources.
    }
}