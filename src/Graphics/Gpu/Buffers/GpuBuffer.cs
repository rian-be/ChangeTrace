using System.Runtime.InteropServices;
using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents GPU buffer used to store shader-accessible structured data.
/// </summary>
/// <typeparam name="T">
/// The unmanaged element type stored in the buffer.
/// </typeparam>
internal sealed class GpuBuffer<T> : IDisposable where T : unmanaged
{
    /// <summary>
    /// Gets the OpenGL handle of the buffer.
    /// </summary>
    internal int Handle { get; }
    
    private readonly BufferTarget _target;

    internal GpuBuffer(BufferTarget target = BufferTarget.ArrayBuffer)
    {
        _target = target;
        Handle  = GL.GenBuffer();
    }

    /// <summary>
    /// Binds a buffer to a configured OpenGL target.
    /// </summary>
    internal void Bind() => GL.BindBuffer(_target, Handle);

    /// <summary>
    /// Uploads structured data to GPU memory.
    /// </summary>
    /// <param name="data">Source data span to upload.</param>
    /// <param name="hint">Expected GPU usage pattern</param>
    internal void Upload(ReadOnlySpan<T> data, BufferUsageHint hint = BufferUsageHint.DynamicDraw)
    {
        Bind();
        unsafe
        {
            fixed (T* ptr = data)
                GL.BufferData(_target, data.Length * sizeof(T), (IntPtr)ptr, hint);
        }
    }

    /// <summary>
    /// Allocates uninitialized GPU storage for a specified element count.
    /// </summary>
    /// <param name="count">
    /// Number of elements to allocate.
    /// </param>
    /// <param name="hint">
    /// Expected GPU usage pattern.
    /// </param>
    internal void UploadEmpty(int count, BufferUsageHint hint = BufferUsageHint.DynamicDraw)
    {
        Bind();
        GL.BufferData(_target, count * Marshal.SizeOf<T>(), IntPtr.Zero, hint);
    }

    public void Dispose() =>
        GL.DeleteBuffer(Handle);
}