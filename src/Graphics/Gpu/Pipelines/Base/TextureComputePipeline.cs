using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Pipelines.Base;

/// <summary>
/// Base pipeline for compute-driven texture generation or processing.
/// </summary>
internal abstract class TextureComputePipeline<TData>(
    int width,
    int height,
    int textureHandle,
    ComputeShader shader)
    : IDisposable
    where TData : unmanaged
{
    /// <summary>
    /// Compute shader used by the pipeline.
    /// </summary>
    protected readonly ComputeShader Shader = shader;

    /// <summary>
    /// Input data uploaded to GPU storage.
    /// </summary>
    protected readonly ShaderStorageBuffer<TData> InputStorageBuffer =
        new();

    /// <summary>
    /// Texture handle processed by the compute shader.
    /// </summary>
    protected readonly int TextureHandle = textureHandle;

    /// <summary>
    /// Texture width in pixels.
    /// </summary>
    private readonly int _width = width;

    /// <summary>
    /// Texture height in pixels.
    /// </summary>
    private readonly int _height = height;

    /// <summary>
    /// Number of active input items.
    /// </summary>
    protected int ItemCount;

    /// <summary>
    /// Dispatches compute work over the texture area.
    /// </summary>
    protected void Dispatch2D(
        int localSizeX = 16,
        int localSizeY = 16)
    {
        GL.DispatchCompute(
            (_width + localSizeX - 1) / localSizeX,
            (_height + localSizeY - 1) / localSizeY,
            1);
    }

    /// <summary>
    /// Binds the pipeline texture as an image texture.
    /// </summary>
    protected void BindImageTexture(
        int binding,
        TextureAccess access,
        SizedInternalFormat format)
    {
        GL.BindImageTexture(
            binding,
            TextureHandle,
            0,
            false,
            0,
            access,
            format);
    }

    /// <summary>
    /// Unbinds an image texture binding.
    /// </summary>
    protected void UnbindImageTexture(
        int binding,
        TextureAccess access,
        SizedInternalFormat format)
    {
        GL.BindImageTexture(
            binding,
            0,
            0,
            false,
            0,
            access,
            format);
    }

    /// <summary>
    /// Synchronizes shader storage and texture access.
    /// </summary>
    protected void Barrier()
    {
        const MemoryBarrierFlags barriers =
            (MemoryBarrierFlags)(
                MemoryBarrierFlags.ShaderStorageBarrierBit |
                MemoryBarrierFlags.ShaderImageAccessBarrierBit |
                MemoryBarrierFlags.TextureFetchBarrierBit);

        GL.MemoryBarrier(barriers);
    }

    /// <summary>
    /// Binds the input storage buffer to a shader binding point.
    /// </summary>
    protected void BindInputBuffer(int binding = 0)
    {
        InputStorageBuffer.BindBase(binding);
    }

    /// <summary>
    /// Unbinds input storage buffer.
    /// </summary>
    protected void UnbindInputBuffer(int binding = 0)
    {
        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            binding,
            0);
    }

    public virtual void Dispose()
    {
        InputStorageBuffer.Dispose();
        GL.DeleteTexture(TextureHandle);
    }
}