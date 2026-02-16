using ChangeTrace.Core;
using ChangeTrace.Core.Results;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Persists <see cref="Timeline"/> instances to storage and retrieves them.
/// Abstracts over storage format (MessagePack, etc.).
/// </summary>
internal interface ITimelineRepository
{
    /// <summary>
    /// Saves the given timeline to the specified file path.
    /// </summary>
    /// <param name="timeline">Timeline to persist.</param>
    /// <param name="filePath">Full path to the target file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure of the save operation.
    /// </returns>
    Task<Result> SaveAsync(Timeline timeline, string filePath, CancellationToken cancellationToken = default);

    /// <summary>
    /// Loads a timeline from the specified file path.
    /// </summary>
    /// <param name="filePath">Full path to the source file.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the loaded timeline or an error if the operation failed.
    /// </returns>
    Task<Result<Timeline>> LoadAsync(string filePath, CancellationToken cancellationToken = default);
}