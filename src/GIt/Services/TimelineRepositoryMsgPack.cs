using ChangeTrace.Core;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Repository for persisting <see cref="Timeline"/> objects using a MessagePack serializer.
/// Encapsulates file handling, serialization, and logging.
/// </summary>
internal sealed class TimelineRepositoryMsgPack(
    ILogger<TimelineRepositoryMsgPack> logger,
    ITimelineSerializer serializer,
    IFileManager fileManager)
    : ITimelineRepository
{
    private const string FileExtension = ".gittrace";

    /// <summary>
    /// Saves a timeline to a file using the configured serializer and file manager.
    /// </summary>
    /// <param name="timeline">Timeline to persist.</param>
    /// <param name="filePath">Destination file path.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><see cref="Result"/> indicating success or failure.</returns>
    public async Task<Result> SaveAsync(Timeline timeline, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            filePath = fileManager.EnsureExtension(filePath, FileExtension);
            logger.LogInformation("Saving timeline to {Path}", filePath);

            var bytes = await serializer.SerializeAsync(timeline, cancellationToken);
            await fileManager.SaveAsync(filePath, bytes, cancellationToken);

            logger.LogInformation("Timeline saved successfully ({Length} bytes)", bytes.Length);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save timeline");
            return Result.Failure("Failed to save timeline", ex);
        }
    }

    /// <summary>
    /// Loads a timeline from a file using the configured serializer and file manager.
    /// </summary>
    /// <param name="filePath">File path to load from.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns><see cref="Result{Timeline}"/> containing the loaded timeline or failure.</returns>
    public async Task<Result<Timeline>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Loading timeline from {Path}", filePath);

            var bytes = await fileManager.LoadAsync(filePath, cancellationToken);
            var timeline = await serializer.DeserializeAsync(bytes, cancellationToken);

            logger.LogInformation("Timeline loaded: {Count} events", timeline.Count);
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load timeline");
            return Result<Timeline>.Failure("Failed to load timeline", ex);
        }
    }
}