using System.Buffers;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Utils;

/// <summary>
/// Represents pooled collection of deleted branches returned by <c>BranchTracker</c>.
/// </summary>
/// <remarks>
/// <para>
/// The underlying array is rented from <see cref="ArrayPool{T}.Shared"/> to minimize
/// allocations when returning collections of deleted branches.
/// </para>
/// <para>
/// This type is implemented as <c>ref struct</c> to enforce stackonly usage and
/// prevent accidental heap allocation or escaping references.
/// </para>
/// <para>
/// Consumers must call <see cref="Dispose"/> after finishing enumeration in order
/// to return the rented array to the pool.
/// </para>
/// </remarks>
internal readonly ref struct PooledDeletedBranches
{
    private readonly (string Name, CommitSha LastSha, Timestamp LastTimestamp)[]? _array;
    private readonly int _count;
    
    /// <param name="array">
    /// The array rented from the shared <see cref="ArrayPool{T}"/> containing deleted branch entries.
    /// </param>
    /// <param name="count">
    /// The number of valid entries stored in the rented array.
    /// </param>
    internal PooledDeletedBranches((string Name, CommitSha LastSha, Timestamp LastTimestamp)[] array, int count)
    {
        _array = array;
        _count = count;
    }

    /// <summary>
    /// Gets readonly span containing deleted branch entries with SHA and timestamp.
    /// </summary>
    /// <remarks>
    /// The span exposes only valid portion of rented array.
    /// If no array was rented, an empty span is returned.
    /// </remarks>
    public ReadOnlySpan<(string Name, CommitSha LastSha, Timestamp LastTimestamp)> Span 
        => _array is null ? default : new ReadOnlySpan<(string, CommitSha, Timestamp)>(_array, 0, _count);

    /// <summary>
    /// Returns the rented array to shared array pool.
    /// </summary>
    /// <remarks>
    /// After calling this method <see cref="Span"/> should no longer be used.
    /// </remarks>
    public void Dispose()
    {
        if (_array is not null)
            ArrayPool<(string, CommitSha, Timestamp)>.Shared.Return(_array, clearArray: true);
    }
}