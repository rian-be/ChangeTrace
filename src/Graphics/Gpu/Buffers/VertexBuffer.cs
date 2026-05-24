using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents GPU vertex buffer storing structured vertex data.
/// </summary>
/// <typeparam name="T">
/// The unmanaged vertex element type.
/// </typeparam>
internal sealed class VertexBuffer<T> : IDisposable
    where T : unmanaged
{
    private readonly GpuBuffer<T> _buffer = new();

    /// <summary>
    /// Gets underlying OpenGL buffer handle.
    /// </summary>
    internal int Handle => _buffer.Handle;

    /// <summary>
    /// Binds buffer to <see cref="BufferTarget.ArrayBuffer"/>.
    /// </summary>
    internal void Bind() => _buffer.Bind();

    /// <summary>
    /// Uploads vertex data to GPU memory.
    /// </summary>
    /// <param name="data"> Source vertex data to upload. </param>
    /// <param name="usage"> Expected GPU usage pattern. </param>
    internal void Upload(
        ReadOnlySpan<T> data,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw) =>
        _buffer.Upload(data, usage);

    /// <summary>
    /// Updates existing GPU vertex buffer contents.
    /// </summary>
    /// <param name="data">Data to upload into GPU memory.</param>
    /// <param name="offsetElements">Element offset inside GPU buffer.</param>
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
                    BufferTarget.ArrayBuffer,
                    offsetElements * sizeof(T),
                    data.Length * sizeof(T),
                    (IntPtr)ptr);
            }
        }
    }
    
    /// <summary>
    /// Allocates uninitialized GPU storage for vertex data.
    /// </summary>
    /// <param name="count"> Number of vertex elements to allocate. </param>
    /// <param name="usage"> Expected GPU usage pattern. </param>
    internal void UploadEmpty(
        int count,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw) =>
        _buffer.UploadEmpty(count, usage);
    
    public void Dispose() => _buffer.Dispose();
}