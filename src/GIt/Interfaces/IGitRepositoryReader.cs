using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Options;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Abstraction for Git repository access.
/// <para>
/// Provides methods to read commits and clone repositories. 
/// This interface is host agnostic: it operates on local Git repos and does not depend on platform APIs (GitHub, GitLab, etc.).
/// </para>
/// </summary>
internal interface IGitRepositoryReader
{
    /// <summary>
    /// Reads all commits from local Git repository.
    /// </summary>
    /// <param name="repositoryPath">Path to the local repository.</param>
    /// <param name="options">Options controlling which commits to read and additional details (e.g., file changes, date filters).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{T}"/> containing a read-only list of <see cref="CommitData"/> on success,
    /// or a failure result with an error message/exception.
    /// </returns>
    Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
        string repositoryPath,
        GitReaderOptions options,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Clones a remote Git repository to a local path.
    /// </summary>
    /// <param name="url">Remote repository URL.</param>
    /// <param name="destinationPath">Local destination path where the repository will be cloned.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure. Failure includes exception details if cloning fails.
    /// </returns>
    Task<Result> CloneAsync(
        string url,
        string destinationPath,
        CancellationToken cancellationToken = default);
}