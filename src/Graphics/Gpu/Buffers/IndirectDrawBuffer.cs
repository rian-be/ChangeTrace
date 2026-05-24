using OpenTK.Graphics.OpenGL4;

namespace ChangeTrace.Graphics.Gpu.Buffers;

/// <summary>
/// Represents GPU buffer storing indirect draw commands.
/// </summary>
/// <typeparam name="TCommand">
/// The unmanaged indirect draw command structure type.
/// </typeparam>
internal sealed class IndirectDrawBuffer<TCommand> : IDisposable
    where TCommand : unmanaged
{
    private readonly GpuBuffer<TCommand> _buffer =
        new(BufferTarget.DrawIndirectBuffer);

    /// <summary>
    /// Gets underlying OpenGL buffer handle.
    /// </summary>
    internal int Handle => _buffer.Handle;

    /// <summary>
    /// Binds buffer to <see cref="BufferTarget.DrawIndirectBuffer"/>.
    /// </summary>
    internal void Bind() => _buffer.Bind();

    /// <summary>
    /// Uploads a single indirect draw command to GPU memory.
    /// </summary>
    /// <param name="command">
    /// Indirect draw command structure.
    /// </param>
    /// <param name="usage">
    /// Expected GPU usage pattern.
    /// </param>
    internal void UploadCommand(
        in TCommand command,
        BufferUsageHint usage = BufferUsageHint.DynamicDraw)
    {
        Bind();

        unsafe
        {
            fixed (TCommand* ptr = &command)
            {
                GL.BufferData(
                    BufferTarget.DrawIndirectBuffer,
                    sizeof(TCommand),
                    (IntPtr)ptr,
                    usage);
            }
        }
    }

    /// <summary>
    /// Updates the portion of indirect draw buffer memory.
    /// </summary>
    /// <typeparam name="TValue">
    /// The unmanaged value type to upload.
    /// </typeparam>
    /// <param name="byteOffset">
    /// Byte offset inside GPU buffer.
    /// </param>
    /// <param name="value">
    /// Value to write into GPU memory.
    /// </param>
    internal void SubData<TValue>(
        int byteOffset,
        in TValue value)
        where TValue : unmanaged
    {
        Bind();

        unsafe
        {
            fixed (TValue* ptr = &value)
            {
                GL.BufferSubData(
                    BufferTarget.DrawIndirectBuffer,
                    byteOffset,
                    sizeof(TValue),
                    (IntPtr)ptr);
            }
        }
    }
    
    public void Dispose() => _buffer.Dispose();
}