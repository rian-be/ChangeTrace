using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents GPU shader storage buffer for structured data access in shaders.
/// </summary>
/// <typeparam name="T">
/// The unmanaged element type stored in the buffer.
/// </typeparam>
internal sealed class ShaderStorageBuffer<T> : IDisposable
    where T : unmanaged
{
    private readonly GpuBuffer<T> _buffer =
        new(BufferTarget.ShaderStorageBuffer);

    /// <summary>
    /// Gets underlying OpenGL buffer handle.
    /// </summary>
    internal int Handle => _buffer.Handle;

    /// <summary>
    /// Binds buffer to <see cref="BufferTarget.ShaderStorageBuffer"/>.
    /// </summary>
    internal void Bind() => _buffer.Bind();

    /// <summary>
    /// Binds buffer to shader storage binding point.
    /// </summary>
    /// <param name="binding">
    /// Shader storage binding index.
    /// </param>
    internal void BindBase(int binding) =>
        GL.BindBufferBase(
            BufferRangeTarget.ShaderStorageBuffer,
            binding,
            Handle);

    /// <summary>
    /// Uploads structured data to GPU memory.
    /// </summary>
    /// <param name="data">
    /// Source data to upload.
    /// </param>
    /// <param name="usage">
    /// Expected GPU usage pattern.
    /// </param>
    internal void Upload(
        ReadOnlySpan<T> data,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw) =>
        _buffer.Upload(data, usage);

    /// <summary>
    /// Allocates uninitialized GPU storage for a specified element count.
    /// </summary>
    /// <param name="count">
    /// Number of elements to allocate.
    /// </param>
    /// <param name="usage">
    /// Expected GPU usage pattern.
    /// </param>
    internal void UploadEmpty(
        int count,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw) =>
        _buffer.UploadEmpty(count, usage);

    /// <summary>
    /// Updates portion of shader storage buffer contents.
    /// </summary>
    /// <param name="data"> Data to upload into GPU memory.</param>
    /// <param name="offsetElements"> Element offset inside GPU buffer.</param>
    internal void SubData(
        ReadOnlySpan<T> data,
        int offsetElements = 0)
    {
        Bind();

        unsafe
        {
            fixed (T* ptr = data)
            {
                GL.BufferSubData(
                    BufferTarget.ShaderStorageBuffer,
                    offsetElements * sizeof(T),
                    data.Length * sizeof(T),
                    (IntPtr)ptr);
            }
        }
    }
    
    public void Dispose() => _buffer.Dispose();
}