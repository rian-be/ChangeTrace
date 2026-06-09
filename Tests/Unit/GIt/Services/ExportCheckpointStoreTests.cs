using ChangeTrace.Core.Events;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Checkpoints;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

public sealed class ExportCheckpointStoreTests
{
    [Fact]
    public async Task SaveAndLoadAsync_RoundTripsCheckpointState()
    {
        var serializer = new TestTimelineSerializer();
        var fileManager = new TestFileManager();
        var store = new ExportCheckpointStore(serializer, fileManager);

        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "checkpoint"));

        var root = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Guid.NewGuid().ToString("N"));
        var checkpointKey = Path.Combine(root, "timeline.gittrace");

        try
        {
            await store.Save(
                checkpointKey,
                new ExportCheckpointState("fingerprint", ExportCheckpointStage.EnrichingPullRequests, 4, 0, timeline));

            var loaded = await store.TryLoad(checkpointKey, "fingerprint");

            Assert.NotNull(loaded);
            Assert.Equal("fingerprint", loaded!.Fingerprint);
            Assert.Equal(ExportCheckpointStage.EnrichingPullRequests, loaded.Stage);
            Assert.Equal(4, loaded.NextPullRequestPage);
            Assert.Equal(0, loaded.NextPullRequestIndex);
            Assert.Single(loaded.Timeline.Events);
            Assert.Equal("checkpoint", loaded.Timeline.Events[0].Metadata?.Metadata);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    [Fact]
    public async Task AppendPullRequestPatch_ReplaysPatchedEventsOnLoad()
    {
        var serializer = new TestTimelineSerializer();
        var fileManager = new TestFileManager();
        var store = new ExportCheckpointStore(serializer, fileManager);

        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        var sha = CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value;
        timeline.AddEvent(TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            sha,
            "checkpoint"));

        var root = Path.Combine(Path.GetTempPath(), "ChangeTrace.Tests", Guid.NewGuid().ToString("N"));
        var checkpointKey = Path.Combine(root, "timeline.gittrace");

        try
        {
            await store.Save(
                checkpointKey,
                new ExportCheckpointState("fingerprint", ExportCheckpointStage.Built, 1, 0, timeline));

            var enriched = TraceEventFactory.Commit(
                Timestamp.Create(1_735_689_600).Value,
                ActorName.Create("rian").Value,
                sha,
                "checkpoint")
                .WithPullRequest(
                    PullRequestNumber.Create(42).Value,
                    ChangeTrace.Core.Enums.PullRequestEventType.PullRequestMerged);

            await store.AppendPullRequestPatch(
                checkpointKey,
                new ExportCheckpointState("fingerprint", ExportCheckpointStage.EnrichingPullRequests, 1, 1, timeline),
                0,
                enriched);

            var loaded = await store.TryLoad(checkpointKey, "fingerprint");

            Assert.NotNull(loaded);
            Assert.Single(loaded!.Timeline.Events);
            Assert.Equal(42, loaded.Timeline.Events[0].PullRequest?.Number.Value);
            Assert.Equal(ChangeTrace.Core.Enums.PullRequestEventType.PullRequestMerged, loaded.Timeline.Events[0].PullRequest?.Type);
        }
        finally
        {
            if (Directory.Exists(root))
                Directory.Delete(root, recursive: true);
        }
    }

    private sealed class TestTimelineSerializer : ISerializer<Timeline>
    {
        private Timeline? _timeline;

        public Task<byte[]> SerializeAsync(Timeline obj, CancellationToken ct = default)
        {
            _timeline = obj;
            return Task.FromResult<byte[]>([1, 2, 3]);
        }

        public Task<Timeline> DeserializeAsync(byte[] data, CancellationToken ct = default)
            => Task.FromResult(_timeline ?? new Timeline(null));
    }

    private sealed class TestFileManager : IFileManager
    {
        public Task SaveAsync(string path, byte[] data, CancellationToken cancellationToken = default)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            File.WriteAllBytes(path, data);
            return Task.CompletedTask;
        }

        public Task<byte[]> LoadAsync(string path, CancellationToken cancellationToken = default)
            => Task.FromResult(File.ReadAllBytes(path));

        public Task<Stream> OpenWriteAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public Task<Stream> OpenReadAsync(string path, CancellationToken cancellationToken = default)
            => throw new NotSupportedException();

        public bool Exists(string path) => File.Exists(path);

        public Task<string> ReadAllTextAsync(string path, string fallback = "", CancellationToken ct = default)
            => Task.FromResult(fallback);

        public string EnsureExtension(string path, string extension)
            => path.EndsWith(extension, StringComparison.OrdinalIgnoreCase) ? path : path + extension;

        public IEnumerable<string> FindFiles(string directory, string extension) => [];
    }
}
