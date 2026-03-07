using System;
using System.Buffers;

namespace ChangeTrace.Core.Utils;

/// <summary>
/// Represents pooled buffer for items of type <typeparamref name="T"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Internally rents arrays from <see cref="ArrayPool{T}"/> to minimize allocations.</item>
/// <item>Automatically grows the buffer when capacity is exceeded.</item>
/// <item>Supports clearing and obtaining a <see cref="ReadOnlyMemory{T}"/> view of the current contents.</item>
/// <item>Disposing the buffer returns the array to the pool and invalidates the instance.</item>
/// </list>
/// </remarks>
/// <typeparam name="T">The type of elements stored in the buffer.</typeparam>
internal sealed class PooledBuffer<T>(int initialCapacity = 128) : IDisposable
{
    private bool _disposed = false;

    /// <summary>
    /// Gets the number of items currently stored in the buffer.
    /// </summary>
    public int Count { get; private set; } = 0;

    private T[] Buffer { get; set; } = ArrayPool<T>.Shared.Rent(initialCapacity);

    /// <summary>
    /// Adds an item to the buffer, growing the internal array if necessary.
    /// </summary>
    /// <param name="item">The item to add.</param>
    /// <exception cref="ObjectDisposedException">Thrown if the buffer has been disposed.</exception>
    public void Add(T item)
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledBuffer<T>));
        EnsureCapacity(Count + 1);
        Buffer[Count++] = item;
    }

    /// <summary>
    /// Clears all items from the buffer and resets <see cref="Count"/> to zero.
    /// </summary>
    /// <exception cref="ObjectDisposedException">Thrown if the buffer has been disposed.</exception>
    public void Clear()
    {
        if (_disposed) throw new ObjectDisposedException(nameof(PooledBuffer<T>));
        Array.Clear(Buffer, 0, Count);
        Count = 0;
    }

    /// <summary>
    /// Returns readonly memory view of the current contents of the buffer.
    /// </summary>
    /// <returns>A <see cref="ReadOnlyMemory{T}"/> representing the items in the buffer.</returns>
    public ReadOnlyMemory<T> AsMemory() => Buffer.AsMemory(0, Count);

    /// <summary>
    /// Ensures that the internal buffer has at least the specified capacity.
    /// If not, allocates new arrays from the pool and copies existing items.
    /// </summary>
    /// <param name="required">The required capacity.</param>
    private void EnsureCapacity(int required)
    {
        if (Buffer.Length >= required) return;

        int newSize = Math.Max(Buffer.Length * 2, required);
        var newBuffer = ArrayPool<T>.Shared.Rent(newSize);
        Array.Copy(Buffer, newBuffer, Count);
        ArrayPool<T>.Shared.Return(Buffer, clearArray: true);
        Buffer = newBuffer;
    }

    /// <summary>
    /// Disposes the buffer, returning the internal array to the pool and invalidating the instance.
    /// </summary>
    public void Dispose()
    {
        if (_disposed) return;
        ArrayPool<T>.Shared.Return(Buffer, clearArray: true);
        Buffer = null!;
        Count = 0;
        _disposed = true;
    }
}