using System.Buffers;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MessagePack;

namespace ChangeTrace.GIt.Services;

/// <summary>
/// Saves and loads timelines using MsgPack.
/// Handles .gittrace persistence and streaming export writes.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed partial class TimelineRepositoryMsgPack(
    ILogger<TimelineRepositoryMsgPack> logger,
    ISerializer<Timeline> serializer,
    IFileManager fileManager)
    : ITimelineRepository, IStreamingTimelineRepository
{
    private const string FileExtension = ".gittrace";

    /// <summary>
    /// Saves a timeline to disk.
    /// </summary>
    public async Task<Result> SaveAsync(Timeline timeline, string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            filePath = fileManager.EnsureExtension(filePath, FileExtension);
            logger.LogInformation("Saving timeline to {Path}", filePath);
            var byteLength = await WriteTimelineAsync(filePath, timeline, cancellationToken);

            logger.LogInformation("Timeline saved successfully ({Length} bytes)", byteLength);

            if (ShouldWriteDebugSnapshot(timeline))
                await WriteDebugSnapshotAsync(timeline, filePath, cancellationToken);

            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to save timeline");
            return Result.Failure("Failed to save timeline", ex);
        }
    }

    /// <summary>
    /// Streams commits into a timeline file.
    /// </summary>
    public async Task<Result> SaveAsync(
        IAsyncEnumerable<CommitData> commits,
        string filePath,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken = default)
    {
        string? tempEventsPath = null;

        try
        {
            filePath = fileManager.EnsureExtension(filePath, FileExtension);
            logger.LogInformation("Streaming timeline save to {Path}", filePath);

            tempEventsPath = Path.Combine(
                Path.GetTempPath(),
                "ChangeTrace",
                "timeline-events",
                $"{Guid.NewGuid():N}.bin");

            var eventCount = await WriteEventPayloadAsync(
                commits,
                tempEventsPath,
                options,
                cancellationToken);

            await using var outputStream = await fileManager.OpenWriteAsync(filePath, cancellationToken);
            await WriteTimelinePayloadAsync(
                outputStream,
                tempEventsPath,
                eventCount,
                options.RepositoryId,
                cancellationToken);

            logger.LogInformation("Timeline streamed successfully ({Count} events)", eventCount);
            return Result.Success();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to stream save timeline");
            return Result.Failure("Failed to save timeline", ex);
        }
        finally
        {
            if (tempEventsPath is not null && File.Exists(tempEventsPath))
                File.Delete(tempEventsPath);
        }
    }

    /// <summary>
    /// Loads a timeline from disk.
    /// </summary>
    public async Task<Result<Timeline>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
    {
        try
        {
            logger.LogInformation("Loading timeline from {Path}", filePath);

            Timeline timeline;

            if (serializer is IStreamingSerializer<Timeline> streamingSerializer)
            {
                await using var stream = await fileManager.OpenReadAsync(filePath, cancellationToken);
                timeline = await streamingSerializer.DeserializeAsync(stream, cancellationToken);
            }
            else
            {
                var bytes = await fileManager.LoadAsync(filePath, cancellationToken);
                timeline = await serializer.DeserializeAsync(bytes, cancellationToken);
            }

            logger.LogInformation("Timeline loaded: {Count} events", timeline.Count);
            return Result<Timeline>.Success(timeline);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to load timeline");
            return Result<Timeline>.Failure("Failed to load timeline", ex);
        }
    }

    private async Task<long> WriteTimelineAsync(
        string filePath,
        Timeline timeline,
        CancellationToken cancellationToken)
    {
        if (serializer is IStreamingSerializer<Timeline> streamingSerializer)
            return await WriteTimelineStreamAsync(filePath, timeline, streamingSerializer, cancellationToken);

        return await WriteTimelineBufferAsync(filePath, timeline, cancellationToken);
    }

    private async Task<long> WriteTimelineStreamAsync(
        string filePath,
        Timeline timeline,
        IStreamingSerializer<Timeline> streamingSerializer,
        CancellationToken cancellationToken)
    {
        await using var stream = await fileManager.OpenWriteAsync(filePath, cancellationToken);
        await streamingSerializer.SerializeAsync(stream, timeline, cancellationToken);
        await stream.FlushAsync(cancellationToken);

        return stream.CanSeek ? stream.Length : -1;
    }

    private async Task<long> WriteTimelineBufferAsync(
        string filePath,
        Timeline timeline,
        CancellationToken cancellationToken)
    {
        var bytes = await serializer.SerializeAsync(timeline, cancellationToken);
        await fileManager.SaveAsync(filePath, bytes, cancellationToken);
        return bytes.Length;
    }

    private async Task<int> WriteEventPayloadAsync(
        IAsyncEnumerable<CommitData> commits,
        string tempEventsPath,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken)
    {
        await using var tempStream = await fileManager.OpenWriteAsync(tempEventsPath, cancellationToken);
        var writer = new EventPayloadWriter(tempStream);
        var branchTracker = options.IncludeBranchEvents
            ? new BranchTracker()
            : null;
        var commitCount = 0;

        await foreach (var commit in commits.WithCancellation(cancellationToken))
        {
            TimelineEventEmitter.EmitCommitEvents(
                commit,
                options,
                branchTracker,
                writer.Write);

            commitCount++;
            if (commitCount % 50000 == 0)
                logger.LogInformation("Stream-saved {Count} commits...", commitCount);
        }

        await tempStream.FlushAsync(cancellationToken);
        return writer.EventCount;
    }

    private static async Task WriteTimelinePayloadAsync(
        Stream outputStream,
        string tempEventsPath,
        int eventCount,
        RepositoryId? repositoryId,
        CancellationToken cancellationToken)
    {
        var buffer = new ArrayBufferWriter<byte>(256);
        var writer = new MessagePackWriter(buffer);
        writer.WriteArrayHeader(4);
        writer.WriteNil();
        TimelineMessagePackFormatter.WriteRepositoryId(ref writer, repositoryId);
        writer.WriteArrayHeader(eventCount);
        writer.Flush();

        await outputStream.WriteAsync(buffer.WrittenMemory, cancellationToken);

        await using (var tempReadStream = new FileStream(
                         tempEventsPath,
                         FileMode.Open,
                         FileAccess.Read,
                         FileShare.Read,
                         131072,
                         FileOptions.Asynchronous | FileOptions.SequentialScan))
        {
            await tempReadStream.CopyToAsync(outputStream, 131072, cancellationToken);
        }

        buffer.Clear();
        writer = new MessagePackWriter(buffer);
        writer.Write(false);
        writer.Flush();
        await outputStream.WriteAsync(buffer.WrittenMemory, cancellationToken);
        await outputStream.FlushAsync(cancellationToken);
    }

    private sealed class EventPayloadWriter(Stream stream)
    {
        private readonly ArrayBufferWriter<byte> _buffer = new(1024);

        public int EventCount { get; private set; }

        public void Write(TraceEvent traceEvent)
        {
            _buffer.Clear();
            var writer = new MessagePackWriter(_buffer);
            TimelineMessagePackFormatter.WriteEvent(ref writer, traceEvent);
            writer.Flush();
            stream.Write(_buffer.WrittenSpan);
            EventCount++;
        }
    }
}
