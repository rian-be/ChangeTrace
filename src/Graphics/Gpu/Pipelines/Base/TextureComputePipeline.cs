using ChangeTrace.Graphics.Gpu.Buffers;
using ChangeTrace.Graphics.Shaders.Runtime;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Pipelines.Base;

/// <summary>
/// Base pipeline for compute-driven texture generation or modification.
/// </summary>
/// <typeparam name="TData">
/// The unmanaged GPU input data type consumed by the compute shader.
/// </typeparam>
internal abstract class TextureComputePipeline<TData>(
    int width,
    int height,
    int textureHandle,
    ComputeShader shader)
    : IDisposable
    where TData : unmanaged
{
    /// <summary>
    /// Compute shader used by this pipeline.
    /// </summary>
    protected readonly ComputeShader Shader = shader;

    /// <summary>
    /// Input data buffer consumed by compute shader.
    /// </summary>
    protected readonly ShaderStorageBuffer<TData> InputSsbo =
        new();

    /// <summary>
    /// OpenGL texture handle written or read by compute shader.
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
    /// Dispatches compute a shader over a texture-sized two-dimensional work grid.
    /// </summary>
    /// <param name="localSizeX"> Local work group size on X axis. </param>
    /// <param name="localSizeY"> Local work group size on Y axis. </param>
    protected void Dispatch2D(int localSizeX = 16, int localSizeY = 16) =>
        GL.DispatchCompute(
            (_width + localSizeX - 1) / localSizeX,
            (_height + localSizeY - 1) / localSizeY,
            1);

    /// <summary>
    /// Binds pipeline texture as image unit.
    /// </summary>
    /// <param name="binding"> Image unit binding index. </param>
    /// <param name="access"> Texture image access mode. </param>
    /// <param name="format"> Sized image format used for shader access. </param>
    protected void BindImageTexture(
        int binding,
        TextureAccess access,
        SizedInternalFormat format) =>
        GL.BindImageTexture(binding, TextureHandle, 0, false, 0, access, format);

    /// <summary>
    /// Unbinds image texture from a specified image unit.
    /// </summary>
    /// <param name="binding"> Image unit binding index. </param>
    /// <param name="access"> Texture image access mode. </param>
    /// <param name="format"> Sized image format used for shader access. </param>
    protected void UnbindImageTexture(
        int binding,
        TextureAccess access,
        SizedInternalFormat format) =>
        GL.BindImageTexture(binding, 0, 0, false, 0, access, format);

    /// <summary>
    /// Synchronizes shader storage, image writes, and texture fetches.
    /// </summary>
    protected void Barrier()
    {
        GL.MemoryBarrier(
            MemoryBarrierFlags.ShaderStorageBarrierBit |
            MemoryBarrierFlags.ShaderImageAccessBarrierBit |
            MemoryBarrierFlags.TextureFetchBarrierBit);
    }

    /// <summary>
    /// Binds the input storage buffer to a shader binding point.
    /// </summary>
    /// <param name="binding"> Shader storage binding index. </param>
    protected void BindInputBuffer(int binding = 0) =>
        InputSsbo.BindBase(binding);

    /// <summary>
    /// Unbinds input storage buffer from the shader binding point.
    /// </summary>
    /// <param name="binding"> Shader storage binding index. </param>
    protected void UnbindInputBuffer(int binding = 0) =>
        GL.BindBufferBase(BufferRangeTarget.ShaderStorageBuffer, binding, 0);
    
    public virtual void Dispose()
    {
        InputSsbo.Dispose();
        GL.DeleteTexture(TextureHandle);
    }
}