using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.OpenTK.Renderer;

/// <summary>
/// SRP: owns one GL buffer object.
/// DRY: typed upload hides all unsafe pinning.
/// </summary>
internal sealed class GpuBuffer<T> : IDisposable where T : unmanaged
{
    internal int Handle { get; }
    private readonly BufferTarget _target;

    internal GpuBuffer(BufferTarget target = BufferTarget.ArrayBuffer)
    {
        _target = target;
        Handle  = GL.GenBuffer();
    }

    internal void Bind() => GL.BindBuffer(_target, Handle);

    internal void Upload(ReadOnlySpan<T> data, BufferUsageHint hint = BufferUsageHint.DynamicDraw)
    {
        Bind();
        unsafe
        {
            fixed (T* ptr = data)
                GL.BufferData(_target, data.Length * sizeof(T), (IntPtr)ptr, hint);
        }
    }

    internal void UploadEmpty(int count, BufferUsageHint hint = BufferUsageHint.DynamicDraw)
    {
        Bind();
        GL.BufferData(_target, count * System.Runtime.InteropServices.Marshal.SizeOf<T>(), IntPtr.Zero, hint);
    }

    public void Dispose()
    {
        GL.DeleteBuffer(Handle);
    }
}