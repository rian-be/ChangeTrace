using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto.Sidecars;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Sidecars;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

/// <summary>Tests timeline repository persistence behavior around serialization and file access.</summary>
public sealed class TimelineRepositoryMsgPackTests
{
    /// <summary>SaveAsync ensures the gittrace extension, serializes the timeline, and saves bytes.</summary>
    [Fact]
    public async Task SaveAsync_EnsuresExtensionSerializesAndSavesBytes()
    {
        var serializer = new TestTimelineSerializer();
        var fileManager = new TestFileManager();
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        var path = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "timeline");

        var result = await repository.SaveAsync(timeline, path);

        Assert.True(result.IsSuccess, result.Error ?? "no error");
        Assert.Same(timeline, serializer.SerializedTimeline);
        Assert.Contains(fileManager.SavedPaths, saved => saved.Contains(".gittrace.tmp.", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(fileManager.SavedPayloads, payload =>
            payload.Length == 3 && payload[0] == 1 && payload[1] == 2 && payload[2] == 3);
        Assert.True(File.Exists(path + ".gittrace"));
        Assert.True(File.Exists(path + ".gittrace.debug.json"));
    }

    /// <summary>SaveAsync writes a pull request sidecar when the timeline contains PR events.</summary>
    [Fact]
    public async Task SaveAsync_WritesPullRequestSidecarWhenTimelineContainsPullRequests()
    {
        var serializer = new TestTimelineSerializer();
        var fileManager = new TestFileManager();
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreatePullRequest(100));
        var path = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "timeline");

        var result = await repository.SaveAsync(timeline, path);

        Assert.True(result.IsSuccess, result.Error ?? "no error");
        Assert.Contains(fileManager.SavedPaths, saved => saved.Contains("pullrequest.gittrace.tmp.", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(serializer.SerializedTimeline);
        Assert.Null(serializer.SerializedTimeline!.Events[0].PullRequest);
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(path)!, "timeline.gittrace.parts", "pullrequest.gittrace")));
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(path)!, "timeline.gittrace.parts", "pullrequest.gittrace.debug.json")));
    }

    /// <summary>SaveAsync writes a merge sidecar when the timeline contains merge events.</summary>
    [Fact]
    public async Task SaveAsync_WritesMergeSidecarWhenTimelineContainsMerges()
    {
        var serializer = new TestTimelineSerializer();
        var fileManager = new TestFileManager();
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreateMerge(100));
        var path = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "timeline");

        var result = await repository.SaveAsync(timeline, path);

        Assert.True(result.IsSuccess, result.Error ?? "no error");
        Assert.Contains(fileManager.SavedPaths, saved => saved.Contains("merge.gittrace.tmp.", StringComparison.OrdinalIgnoreCase));
        Assert.Null(serializer.SerializedTimeline!.Events[0].Branch);
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(path)!, "timeline.gittrace.parts", "merge.gittrace")));
        Assert.True(File.Exists(Path.Combine(Path.GetDirectoryName(path)!, "timeline.gittrace.parts", "merge.gittrace.debug.json")));
    }

    /// <summary>LoadAsync loads bytes from the requested path and deserializes them into a timeline.</summary>
    [Fact]
    public async Task LoadAsync_LoadsBytesAndDeserializesTimeline()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        var serializer = new TestTimelineSerializer { TimelineToDeserialize = timeline };
        var fileManager = new TestFileManager { BytesToLoad = [9, 8, 7] };
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var result = await repository.LoadAsync("/tmp/source.gittrace");

        Assert.True(result.IsSuccess);
        Assert.Same(timeline, result.Value);
        Assert.Equal("/tmp/source.gittrace", fileManager.LoadedPath);
        Assert.Equal([9, 8, 7], serializer.DeserializedBytes);
    }

    /// <summary>LoadAsync applies sidecar pull request data to the loaded timeline.</summary>
    [Fact]
    public async Task LoadAsync_AppliesPullRequestSidecarAttachments()
    {
        var baseTimeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        baseTimeline.AddEvent(CreateCommit(100));

        var serializer = new TestTimelineSerializer { TimelineToDeserialize = baseTimeline };
        var fileManager = new TestFileManager { BytesToLoad = [9, 8, 7] };
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var sourcePath = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "source.gittrace");
        var sidecarPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, "source.gittrace.parts", "pullrequest.gittrace");
        var sidecar = PullRequestSidecarDto.FromDomain(CreateTimelineWithPullRequest());
        Directory.CreateDirectory(Path.GetDirectoryName(sidecarPath)!);
        File.WriteAllBytes(sidecarPath, sidecar.ToBytes());
        fileManager.SetLoadBytes(sourcePath, [9, 8, 7]);
        fileManager.SetLoadBytes(sidecarPath, sidecar.ToBytes());

        var result = await repository.LoadAsync(sourcePath);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Events);
        var evt = result.Value.Events[0];
        Assert.NotNull(evt.PullRequest);
        Assert.Equal(7, evt.PullRequest?.Number.Value);
        Assert.Equal(PullRequestEventType.PullRequestMerged, evt.PullRequest?.Type);
    }

    /// <summary>LoadAsync applies merge sidecar attachments to the loaded timeline.</summary>
    [Fact]
    public async Task LoadAsync_AppliesMergeSidecarAttachments()
    {
        var baseTimeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        baseTimeline.AddEvent(CreateCommit(100));

        var serializer = new TestTimelineSerializer { TimelineToDeserialize = baseTimeline };
        var fileManager = new TestFileManager { BytesToLoad = [9, 8, 7] };
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var sourcePath = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "source.gittrace");
        var sidecarPath = Path.Combine(Path.GetDirectoryName(sourcePath)!, "source.gittrace.parts", "merge.gittrace");
        var sidecar = MergeSidecarDto.FromDomain(CreateTimelineWithMerge());
        Directory.CreateDirectory(Path.GetDirectoryName(sidecarPath)!);
        File.WriteAllBytes(sidecarPath, sidecar.ToBytes());
        fileManager.SetLoadBytes(sourcePath, [9, 8, 7]);
        fileManager.SetLoadBytes(sidecarPath, sidecar.ToBytes());

        var result = await repository.LoadAsync(sourcePath);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value.Events);
        var evt = result.Value.Events[0];
        Assert.NotNull(evt.Branch);
        Assert.Equal(BranchEventType.Merge, evt.Branch?.Type);
        Assert.Equal("main", evt.Target);
    }

    /// <summary>SaveAsync wraps serializer failures in a failed Result.</summary>
    [Fact]
    public async Task SaveAsync_ReturnsFailureWhenSerializerThrows()
    {
        var serializer = new TestTimelineSerializer
        {
            SerializeException = new InvalidOperationException("serializer failed")
        };
        var fileManager = new TestFileManager();
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            fileManager,
            new PullRequestSidecarHandler(NullLogger<PullRequestSidecarHandler>.Instance, fileManager),
            new MergeSidecarHandler(NullLogger<MergeSidecarHandler>.Instance, fileManager));

        var path = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "timeline");
        var finalPath = path + ".gittrace";
        Directory.CreateDirectory(Path.GetDirectoryName(finalPath)!);
        await File.WriteAllBytesAsync(finalPath, [9, 9, 9]);

        var result = await repository.SaveAsync(new Timeline(null), path);

        Assert.True(result.IsFailure);
        Assert.Equal("Failed to save timeline", result.Error);
        Assert.Equal([9, 9, 9], await File.ReadAllBytesAsync(finalPath));
    }

    /// <summary>Serializer test double with configurable serialized and deserialized values.</summary>
    private sealed class TestTimelineSerializer : ISerializer<Timeline>
    {
        /// <summary>Timeline passed to SerializeAsync.</summary>
        public Timeline? SerializedTimeline { get; private set; }

        /// <summary>Bytes passed to DeserializeAsync.</summary>
        public byte[]? DeserializedBytes { get; private set; }

        /// <summary>Timeline returned by DeserializeAsync.</summary>
        public Timeline TimelineToDeserialize { get; init; } = new(null);

        /// <summary>Optional exception thrown by SerializeAsync.</summary>
        public Exception? SerializeException { get; init; }

        /// <summary>Records the timeline and returns deterministic bytes.</summary>
        public Task<byte[]> SerializeAsync(Timeline obj, CancellationToken ct = default)
        {
            if (SerializeException is not null)
                throw SerializeException;

            SerializedTimeline = obj;
            return Task.FromResult<byte[]>([1, 2, 3]);
        }

        /// <summary>Records bytes and returns the configured timeline.</summary>
        public Task<Timeline> DeserializeAsync(byte[] data, CancellationToken ct = default)
        {
            DeserializedBytes = data;
            return Task.FromResult(TimelineToDeserialize);
        }
    }

    /// <summary>File manager test double that records load and save calls.</summary>
    private sealed class TestFileManager : IFileManager
    {
        /// <summary>Path passed to SaveAsync.</summary>
        public string? SavedPath { get; private set; }

        /// <summary>Bytes passed to SaveAsync.</summary>
        public byte[]? SavedBytes { get; private set; }

        /// <summary>All paths passed to SaveAsync.</summary>
        public List<string> SavedPaths { get; } = [];

        /// <summary>All payloads passed to SaveAsync.</summary>
        public List<byte[]> SavedPayloads { get; } = [];

        /// <summary>Path passed to LoadAsync.</summary>
        public string? LoadedPath { get; private set; }

        /// <summary>Bytes returned by LoadAsync.</summary>
        public byte[] BytesToLoad { get; init; } = [];

        /// <summary>Path-specific bytes returned by LoadAsync.</summary>
        private readonly Dictionary<string, byte[]> _bytesByPath = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>Configures bytes for a specific path.</summary>
        public void SetLoadBytes(string path, byte[] bytes)
            => _bytesByPath[path] = bytes;

        /// <summary>Records bytes and creates the target directory for repository debug output.</summary>
        public Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            SavedPath = path;
            SavedBytes = data;
            SavedPaths.Add(path);
            SavedPayloads.Add(data);
            File.WriteAllBytes(path, data);
            return Task.CompletedTask;
        }

        /// <summary>Records the path and returns configured bytes.</summary>
        public Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            LoadedPath = path;
            return Task.FromResult(_bytesByPath.TryGetValue(path, out var bytes) ? bytes : BytesToLoad);
        }

        public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            SavedPath = path;
            Stream stream = new RecordingMemoryStream(bytes =>
            {
                SavedBytes = bytes;
                File.WriteAllBytes(path, bytes);
            });
            return Task.FromResult(stream);
        }

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
        {
            LoadedPath = path;
            Stream stream = new MemoryStream(BytesToLoad, writable: false);
            return Task.FromResult(stream);
        }

        /// <summary>Returns whether a file exists on disk.</summary>
        public bool Exists(string path) => File.Exists(path);

        /// <summary>Returns fallback text for tests that do not cover text reads.</summary>
        public Task<string> ReadAllTextAsync(string path, string fallback = "", CancellationToken ct = default)
            => Task.FromResult(fallback);

        /// <summary>Adds an extension when the path does not already end with it.</summary>
        public string EnsureExtension(string path, string extension)
            => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

        /// <summary>Returns no files for tests that do not cover file discovery.</summary>
        public IEnumerable<string> FindFiles(string directory, string extension) => [];

        private sealed class RecordingMemoryStream(Action<byte[]> onDispose) : MemoryStream
        {
            protected override void Dispose(bool disposing)
            {
                if (disposing)
                {
                    onDispose(ToArray());
                }

                base.Dispose(disposing);
            }
        }
    }

    /// <summary>Creates a pull request event for tests.</summary>
    private static TraceEvent CreatePullRequest(long timestamp)
        => TraceEventFactory.PullRequest(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            PullRequestNumber.Create(7).Value,
            PullRequestEventType.PullRequestMerged,
            BranchName.Create("main").Value,
            "Merge pull request #7");

    /// <summary>Creates a commit event for tests.</summary>
    private static TraceEvent CreateCommit(long timestamp)
        => TraceEventFactory.Commit(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Initial commit");

    /// <summary>Creates a timeline containing a pull request event for sidecar tests.</summary>
    private static Timeline CreateTimelineWithPullRequest()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreatePullRequest(100));
        return timeline;
    }

    /// <summary>Creates a merge event for tests.</summary>
    private static TraceEvent CreateMerge(long timestamp)
        => TraceEventFactory.Merge(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            BranchName.Create("main").Value,
            "Merge feature");

    /// <summary>Creates a timeline containing a merge event for sidecar tests.</summary>
    private static Timeline CreateTimelineWithMerge()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreateMerge(100));
        return timeline;
    }
}
