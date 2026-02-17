using ChangeTrace.Configuration;
using ChangeTrace.Core;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Repository implementation for persisting and loading <see cref="Timeline"/> objects
/// using MsgPack serialization.
/// </summary>
/// <remarks>
/// This class handles the full lifecycle of timeline storage:
/// <list type="bullet">
/// <item>Automatically appends the <c>.gittrace</c> extension when saving or loading files.</item>
/// <item>Delegates serialization to <see cref="ITimelineSerializer"/>.</item>
/// <item>Delegates file I/O to <see cref="IFileManager"/>.</item>
/// <item>Returns <see cref="Result"/> or <see cref="Result{T}"/> objects to encapsulate success or failure without throwing exceptions.</item>
/// <item>Designed as a singleton service for dependency injection with <see cref="ServiceLifetime.Singleton"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class TimelineRepositoryMsgPack(
    ILogger<TimelineRepositoryMsgPack> logger,
    ITimelineSerializer serializer,
    IFileManager fileManager)
    : ITimelineRepository
{
    private const string FileExtension = ".gittrace";

    /// <summary>
    /// Saves the given <paramref name="timeline"/> to the specified <paramref name="filePath"/>.
    /// Automatically creates the directory if it does not exist.
    /// </summary>
    /// <param name="timeline">The timeline to persist.</param>
    /// <param name="filePath">The file path where the timeline should be saved.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result"/> indicating success or failure. 
    /// On failure, contains the error message and exception.
    /// </returns>
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
    /// Loads a <see cref="Timeline"/> from the specified <paramref name="filePath"/>.
    /// </summary>
    /// <param name="filePath">The file path to load the timeline from.</param>
    /// <param name="cancellationToken">Token to cancel the operation.</param>
    /// <returns>
    /// A <see cref="Result{Timeline}"/> containing the loaded timeline on success,
    /// or failure information if loading or deserialization fails.
    /// </returns>
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