using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents GPU index buffer used for indexed rendering.
/// </summary>
/// <typeparam name="T">
/// The unmanaged index element type.
/// </typeparam>
internal sealed class IndexBuffer<T> : IDisposable
    where T : unmanaged
{
    private readonly GpuBuffer<T> _buffer =
        new(BufferTarget.ElementArrayBuffer);

    /// <summary>
    /// Gets underlying OpenGL buffer handle.
    /// </summary>
    public int Handle => _buffer.Handle;

    /// <summary>
    /// Gets the number of uploaded indices.
    /// </summary>
    public int Count { get; private set; }

    /// <summary>
    /// Binds index buffer to <see cref="BufferTarget.ElementArrayBuffer"/>.
    /// </summary>
    public void Bind() =>
        _buffer.Bind();

    /// <summary>
    /// Currently unbinds bound index buffer.
    /// </summary>
    public static void Unbind()
    {
        GL.BindBuffer(
            BufferTarget.ElementArrayBuffer,
            0);
    }

    /// <summary>
    /// Uploads index data to GPU memory.
    /// </summary>
    /// <param name="indices">
    /// Index data to upload.
    /// </param>
    /// <param name="usage">
    /// Expected GPU usage pattern.
    /// </param>
    public void Upload(
        ReadOnlySpan<T> indices,
        BufferUsageHint usage = BufferUsageHint.StaticDraw)
    {
        Count = indices.Length;

        _buffer.Upload(
            indices,
            usage);
    }

    /// <summary>
    /// Allocates uninitialized GPU storage for index data.
    /// </summary>
    /// <param name="count">
    /// Number of indices to allocate.
    /// </param>
    /// <param name="usage">
    /// Expected GPU usage pattern.
    /// </param>
    public void UploadEmpty(
        int count,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw)
    {
        Count = count;

        _buffer.UploadEmpty(
            count,
            usage);
    }

    /// <summary>
    /// Updates existing GPU index buffer contents.
    /// </summary>
    /// <param name="indices">
    /// Index data to upload.
    /// </param>
    public void SubData(
        ReadOnlySpan<T> indices)
    {
        Bind();

        unsafe
        {
            fixed (T* ptr = indices)
            {
                GL.BufferSubData(
                    BufferTarget.ElementArrayBuffer,
                    0,
                    indices.Length * sizeof(T),
                    (IntPtr)ptr);
            }
        }
    }
    
    public void Dispose() =>
        _buffer.Dispose();
}