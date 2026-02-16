using ChangeTrace.Core;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Delegates;
using ChangeTrace.GIt.Options;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Orchestrates exporting Git repository into <see cref="Timeline"/>.
/// Handles cloning, reading commits, building the timeline, optional enrichment, normalization, and persistence.
/// This is the main entry point for repository export operations.
/// </summary>
internal interface IRepositoryExporter
{
    /// <summary>
    /// Exports repository to <see cref="Timeline"/>.
    /// </summary>
    /// <param name="pathOrUrl">Local repository path or remote Git URL (auto cloned if remote).</param>
    /// <param name="options">Options controlling export behavior (date filters, max commits, enrichment, etc.).</param>
    /// <param name="progress">Optional progress callback reporting operation name and percentage (0-100).</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the exported timeline on success, or failure with error details.
    /// </returns>
    Task<Result<Timeline>> ExportAsync(
        string pathOrUrl,
        ExportOptions options,
        ProgressCallback? progress = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Exports  repository and saves the resulting timeline to a file.
    /// </summary>
    /// <param name="pathOrUrl">Local repository path or remote Git URL (auto-cloned if remote).</param>
    /// <param name="outputPath">Path to save the timeline file (.gittrace format).</param>
    /// <param name="options">Options controlling export behavior.</param>
    /// <param name="progress">Optional progress callback reporting export and save progress.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure of export and save operations.
    /// </returns>
    Task<Result> ExportAndSaveAsync(
        string pathOrUrl,
        string outputPath,
        ExportOptions options,
        ProgressCallback? progress = null,
        CancellationToken cancellationToken = default);
}