using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using StbImageSharp;

namespace ChangeTrace.Graphics.Rendering.Renderers.Sprites;

/// <summary>
/// Renders language and file type icons from texture atlas.
/// </summary>
internal sealed class IconSpriteRenderer : IDisposable
{
    /// <summary>
    /// Atlas cell size in pixels.
    /// </summary>
    private const int CellSize = 32;

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

    private readonly VertexArray _vao = new();
    private readonly VertexBuffer<float> _quadVbo = new();

    private readonly int _texture;

    private readonly int _textureWidth;
    private readonly int _textureHeight;
    private readonly int _columns;

    private bool _disposed;

    private IconSpriteRenderer(
        int texture,
        int textureWidth,
        int textureHeight)
    {
        _texture = texture;

        _textureWidth = textureWidth;
        _textureHeight = textureHeight;

        _columns =
            Math.Max(
                1,
                textureWidth / CellSize);

        InitializeGpu();
    }

    /// <summary>
    /// Loads icon atlas texture from a PNG file.
    /// </summary>
    public static IconSpriteRenderer LoadFromPng(
        string path)
    {
        if (!File.Exists(path))
        {
            throw new FileNotFoundException(
                $"Language icon atlas not found: {path}",
                path);
        }

        using Stream stream =
            File.OpenRead(path);

        ImageResult image =
            ImageResult.FromStream(
                stream,
                ColorComponents.RedGreenBlueAlpha);

        int texture =
            GL.GenTexture();

        GL.BindTexture(
            TextureTarget.Texture2D,
            texture);

        GL.PixelStore(
            PixelStoreParameter.UnpackAlignment,
            1);

        GL.TexImage2D(
            TextureTarget.Texture2D,
            0,
            PixelInternalFormat.Rgba,
            image.Width,
            image.Height,
            0,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            image.Data);

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

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        return new IconSpriteRenderer(
            texture,
            image.Width,
            image.Height);
    }

    /// <summary>
    /// Draws a single icon sprite from the atlas.
    /// </summary>
    public void Draw(
        ShaderProgram shader,
        LanguageIcon icon,
        Vector4 rectPx,
        Vector2 viewport,
        Vector4 tint)
    {
        if (_disposed ||
            _texture == 0)
        {
            return;
        }

        GL.Enable(EnableCap.Blend);

        GL.BlendFunc(
            BlendingFactor.SrcAlpha,
            BlendingFactor.OneMinusSrcAlpha);

        shader.Use();

        shader.Set(
            "uRectPx",
            rectPx);

        shader.Set(
            "uUvRect",
            GetUvRect(icon));

        shader.Set(
            "uViewport",
            viewport);

        shader.Set(
            "uTint",
            tint);

        GL.ActiveTexture(
            TextureUnit.Texture0);

        GL.BindTexture(
            TextureTarget.Texture2D,
            _texture);

        shader.Set(
            "uTexture",
            0);

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
    /// Initializes GPU buffers for sprite rendering.
    /// </summary>
    private void InitializeGpu()
    {
        _vao.Bind();

        _quadVbo.Upload(
            QuadVertices,
            BufferUsageHint.StaticDraw);

        _vao.BindVertexBuffer(
            _quadVbo);

        _vao.AttributePointer(
            index: 0,
            componentCount: 2,
            type: VertexAttribPointerType.Float,
            strideBytes: 2 * sizeof(float),
            offsetBytes: 0);

        VertexArray.Unbind();
    }

    /// <summary>
    /// Resolves normalized atlas UV rectangle for an icon.
    /// </summary>
    private Vector4 GetUvRect(
        LanguageIcon icon)
    {
        int index =
            Math.Max(
                0,
                (int)icon);

        int col =
            index % _columns;

        int row =
            index / _columns;

        float u =
            col * CellSize / (float)_textureWidth;

        float v =
            row * CellSize / (float)_textureHeight;

        float uw =
            CellSize / (float)_textureWidth;

        float vh =
            CellSize / (float)_textureHeight;

        return new Vector4(
            u,
            v,
            uw,
            vh);
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _quadVbo.Dispose();
        _vao.Dispose();

        GL.DeleteTexture(_texture);

        _disposed = true;
    }
}