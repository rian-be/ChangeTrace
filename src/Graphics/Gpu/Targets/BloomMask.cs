using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;

namespace ChangeTrace.Graphics.Gpu.Targets;

/// <summary>
/// GPU target for bloom mask extraction.
/// Extracts bright regions into a single-channel bloom texture.
/// </summary>
internal sealed class BloomMask : IDisposable
{
    private readonly ComputeShader _shader;

    private readonly int _inputTexture;
    private readonly int _bloomMaskTexture;

    private readonly Vector2i _size;

    /// <summary>
    /// Generated bloom mask texture handle.
    /// </summary>
    public int Texture =>
        _bloomMaskTexture;

    public BloomMask(
        int width,
        int height,
        ComputeShader shader)
    {
        _size = new Vector2i(
            Math.Max(1, width),
            Math.Max(1, height));

        _shader = shader;

        _inputTexture = CreateInputTexture(
            _size.X,
            _size.Y);

        _bloomMaskTexture = CreateBloomMaskTexture(
            _size.X,
            _size.Y);
    }

    /// <summary>
    /// Uploads RGBA source texture data.
    /// </summary>
    public void UpdateInputTexture(byte[] data)
    {
        GL.BindTexture(
            TextureTarget.Texture2D,
            _inputTexture);

        GL.TexSubImage2D(
            TextureTarget.Texture2D,
            0,
            0,
            0,
            _size.X,
            _size.Y,
            PixelFormat.Rgba,
            PixelType.UnsignedByte,
            data);

        GL.BindTexture(TextureTarget.Texture2D, 0);
    }

    /// <summary>
    /// Generates bloom mask texture from the current input texture.
    /// </summary>
    public void Generate(float bloomThreshold = 0.8f)
    {
        BindImages();

        _shader.Use();
        _shader.Set(
            "bloomThreshold",
            bloomThreshold);

        Dispatch();

        Barrier();
        UnbindImages();
    }

    private void Dispatch()
    {
        GL.DispatchCompute(
            (uint)Math.Ceiling(_size.X / 16.0),
            (uint)Math.Ceiling(_size.Y / 16.0),
            1);
    }

    private static void Barrier()
    {
        const MemoryBarrierFlags barriers =
            MemoryBarrierFlags.ShaderImageAccessBarrierBit;

        GL.MemoryBarrier(barriers);
    }

    private void BindImages()
    {
        GL.BindImageTexture(
            0,
            _inputTexture,
            0,
            false,
            0,
            TextureAccess.ReadOnly,
            SizedInternalFormat.Rgba8);

        GL.BindImageTexture(
            1,
            _bloomMaskTexture,
            0,
            false,
            0,
            TextureAccess.WriteOnly,
            SizedInternalFormat.R8);
    }

    private void UnbindImages()
    {
        GL.BindImageTexture(
            0,
            0,
            0,
            false,
            0,
            TextureAccess.ReadOnly,
            SizedInternalFormat.Rgba8);

        GL.BindImageTexture(
            1,
            0,
            0,
            false,
            0,
            TextureAccess.WriteOnly,
            SizedInternalFormat.R8);
    }

    private static int CreateInputTexture(
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

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        return texture;
    }

    private static int CreateBloomMaskTexture(
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
            PixelInternalFormat.R8,
            width,
            height,
            0,
            PixelFormat.Red,
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

        GL.BindTexture(
            TextureTarget.Texture2D,
            0);

        return texture;
    }

    public void Dispose()
    {
        GL.DeleteTexture(_inputTexture);
        GL.DeleteTexture(_bloomMaskTexture);
    }
}