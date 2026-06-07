using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Services;
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
            fileManager);
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        var path = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Ulid.NewUlid().ToString(), "timeline");

        var result = await repository.SaveAsync(timeline, path);

        Assert.True(result.IsSuccess);
        Assert.Same(timeline, serializer.SerializedTimeline);
        Assert.Equal(path + ".gittrace", fileManager.SavedPath);
        Assert.Equal([1, 2, 3], fileManager.SavedBytes);
        Assert.True(File.Exists(path + ".gittrace.debug.json"));
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
            fileManager);

        var result = await repository.LoadAsync("/tmp/source.gittrace");

        Assert.True(result.IsSuccess);
        Assert.Same(timeline, result.Value);
        Assert.Equal("/tmp/source.gittrace", fileManager.LoadedPath);
        Assert.Equal([9, 8, 7], serializer.DeserializedBytes);
    }

    /// <summary>SaveAsync wraps serializer failures in a failed Result.</summary>
    [Fact]
    public async Task SaveAsync_ReturnsFailureWhenSerializerThrows()
    {
        var serializer = new TestTimelineSerializer
        {
            SerializeException = new InvalidOperationException("serializer failed")
        };
        var repository = new TimelineRepositoryMsgPack(
            NullLogger<TimelineRepositoryMsgPack>.Instance,
            serializer,
            new TestFileManager());

        var result = await repository.SaveAsync(new Timeline(null), "/tmp/timeline");

        Assert.True(result.IsFailure);
        Assert.Equal("Failed to save timeline", result.Error);
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

        /// <summary>Path passed to LoadAsync.</summary>
        public string? LoadedPath { get; private set; }

        /// <summary>Bytes returned by LoadAsync.</summary>
        public byte[] BytesToLoad { get; init; } = [];

        /// <summary>Records bytes and creates the target directory for repository debug output.</summary>
        public Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            SavedPath = path;
            SavedBytes = data;
            return Task.CompletedTask;
        }

        /// <summary>Records the path and returns configured bytes.</summary>
        public Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            LoadedPath = path;
            return Task.FromResult(BytesToLoad);
        }

        public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            SavedPath = path;

            Stream stream = new RecordingMemoryStream(bytes => SavedBytes = bytes);
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
                    onDispose(ToArray());

                base.Dispose(disposing);
            }
        }
    }
}
