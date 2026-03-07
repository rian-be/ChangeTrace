using System.Buffers;
using System.Runtime.CompilerServices;

namespace ChangeTrace.Core.Events.Semantic;

/// <summary>
/// Writer for semantic events of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Internally uses an array rented from <see cref="ArrayPool{T}"/> to minimize allocations.</item>
/// <item>Supports dynamic growth when the buffer is full.</item>
/// <item>Provides <see cref="Snapshot"/> to obtain a <see cref="ReadOnlyMemory{T}"/> view of written events without copying.</item>
/// <item>Calling <see cref="Dispose"/> returns the buffer to the pool and invalidates the writer.</item>
/// </list>
/// </remarks>
/// <typeparam name="T">Type of event stored in the writer.</typeparam>
public sealed class SemanticEventWriter<T>(int initialCapacity = 128) : IDisposable
{
    private T[] _buffer = ArrayPool<T>.Shared.Rent(initialCapacity);
    private int _count = 0;
    private bool _disposed = false;

    /// <summary>
    /// Gets the number of events currently written to the writer.
    /// </summary>
    public int Count => _count;

    /// <summary>
    /// Writes an event to the buffer.
    /// </summary>
    /// <param name="evt">The event to write.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the writer has been disposed.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Write(T evt)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SemanticEventWriter<T>));

        if (_count == _buffer.Length)
            Grow();

        _buffer[_count++] = evt;
    }

    /// <summary>
    /// Returns snapshot of the written events as <see cref="ReadOnlyMemory{T}"/>.
    /// </summary>
    /// <returns>A read-only memory segment containing all written events.</returns>
    /// <exception cref="ObjectDisposedException">Thrown if the writer has been disposed.</exception>
    public ReadOnlyMemory<T> Snapshot() =>
        _disposed
            ? throw new ObjectDisposedException(nameof(SemanticEventWriter<T>))
            : _buffer.AsMemory(0, _count);

    /// <summary>
    /// Clears all written events without releasing the buffer.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the writer has been disposed.</exception>
    public void Clear()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(SemanticEventWriter<T>));
        _count = 0;
    }

    /// <summary>
    /// Disposes a writer, returning it's buffer to the <see cref="ArrayPool{T}"/>.
    /// After disposal, the writer cannot be used.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        ArrayPool<T>.Shared.Return(_buffer, clearArray: false);
        _buffer = null!;
        _count = 0;
        _disposed = true;
    }

    /// <summary>
    /// Doubles buffer size when capacity is exceeded.
    /// </summary>
    private void Grow()
    {
        var newSize = _buffer.Length * 2;
        var newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        Array.Copy(_buffer, newBuffer, _count);
        ArrayPool<T>.Shared.Return(_buffer, clearArray: false);
        _buffer = newBuffer;
    }
}