using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Options;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Reads commit history from a Git repository.
/// </summary>
internal interface ICommitHistoryReaderBackend
{
    /// <summary>
    /// Backend type used by this reader.
    /// </summary>
    GitHistoryReaderBackend Backend { get; }

    /// <summary>
    /// Reads commit history as an asynchronous stream.
    /// </summary>
    Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken);
}
