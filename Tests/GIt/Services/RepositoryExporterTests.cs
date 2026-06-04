using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

/// <summary>Tests repository export orchestration across reader, builder, and repository services.</summary>
public sealed class RepositoryExporterTests
{
    /// <summary>ExportAsync clones remote repositories, forwards reader options, and builds a normalized timeline.</summary>
    [Fact]
    public async Task ExportAsync_ClonesRemoteReadsCommitsAndBuildsTimeline()
    {
        var commit = CreateCommitData(1_735_689_600);
        var reader = new TestGitRepositoryReader { CommitsToReturn = [commit] };
        var builder = new TestTimelineBuilder();
        var exporter = CreateExporter(reader, builder, new TestTimelineRepository());
        var options = new ExportOptions
        {
            IncludeFileChanges = false,
            IncludeBranchEvents = false,
            IncludeMergeDetection = false,
            MaxCommits = 5,
            StartDate = DateTimeOffset.FromUnixTimeSeconds(100),
            EndDate = DateTimeOffset.FromUnixTimeSeconds(200)
        };

        var result = await exporter.ExportAsync("https://github.com/rian-be/ChangeTrace.git", options);

        Assert.True(result.IsSuccess);
        Assert.Equal("https://github.com/rian-be/ChangeTrace.git", reader.ClonedUrl);
        Assert.NotNull(reader.CloneDestinationPath);
        Assert.Equal(reader.CloneDestinationPath, reader.ReadRepositoryPath);
        Assert.Equal(false, reader.ReadOptions?.IncludeFileChanges);
        Assert.Equal(5, reader.ReadOptions?.MaxCommits);
        Assert.Equal(options.StartDate, reader.ReadOptions?.StartDate);
        Assert.Equal(options.EndDate, reader.ReadOptions?.EndDate);
        Assert.Same(reader.CommitsToReturn, builder.Commits);
        Assert.Equal("rian-be", builder.Options?.RepositoryId?.Owner);
        Assert.Equal("ChangeTrace", builder.Options?.RepositoryId?.Name);
        Assert.False(builder.Options?.IncludeFileChanges);
        Assert.False(builder.Options?.IncludeBranchEvents);
        Assert.False(builder.Options?.IncludeMergeDetection);
    }

    /// <summary>ExportAsync returns reader failures without invoking the timeline builder.</summary>
    [Fact]
    public async Task ExportAsync_ReturnsFailureWhenReaderFails()
    {
        var reader = new TestGitRepositoryReader
        {
            ReadResult = Result<IReadOnlyList<CommitData>>.Failure("read failed")
        };
        var builder = new TestTimelineBuilder();
        var exporter = CreateExporter(reader, builder, new TestTimelineRepository());

        var result = await exporter.ExportAsync("https://github.com/rian-be/ChangeTrace.git", new ExportOptions());

        Assert.True(result.IsFailure);
        Assert.Equal("read failed", result.Error);
        Assert.Null(builder.Commits);
    }

    /// <summary>ExportAndSaveAsync saves the exported timeline to the requested output path.</summary>
    [Fact]
    public async Task ExportAndSaveAsync_SavesTimelineAfterSuccessfulExport()
    {
        var reader = new TestGitRepositoryReader { CommitsToReturn = [CreateCommitData(1_735_689_600)] };
        var builder = new TestTimelineBuilder();
        var repository = new TestTimelineRepository();
        var exporter = CreateExporter(reader, builder, repository);

        var result = await exporter.ExportAndSaveAsync(
            "https://github.com/rian-be/ChangeTrace.git",
            "/tmp/output.gittrace",
            new ExportOptions());

        Assert.True(result.IsSuccess);
        Assert.Same(builder.TimelineToReturn, repository.SavedTimeline);
        Assert.Equal("/tmp/output.gittrace", repository.SavedPath);
    }

    /// <summary>ExportAndSaveAsync propagates repository save failures.</summary>
    [Fact]
    public async Task ExportAndSaveAsync_ReturnsFailureWhenSaveFails()
    {
        var reader = new TestGitRepositoryReader { CommitsToReturn = [CreateCommitData(1_735_689_600)] };
        var repository = new TestTimelineRepository { SaveResult = Result.Failure("save failed") };
        var exporter = CreateExporter(reader, new TestTimelineBuilder(), repository);

        var result = await exporter.ExportAndSaveAsync(
            "https://github.com/rian-be/ChangeTrace.git",
            "/tmp/output.gittrace",
            new ExportOptions());

        Assert.True(result.IsFailure);
        Assert.Equal("save failed", result.Error);
    }

    /// <summary>Creates an exporter wired with test doubles.</summary>
    private static RepositoryExporter CreateExporter(
        IGitRepositoryReader reader,
        ITimelineBuilder builder,
        ITimelineRepository repository)
        => new(
            reader,
            builder,
            repository,
            NullLogger<RepositoryExporter>.Instance);

    /// <summary>Creates a commit fixture for repository exporter tests.</summary>
    private static CommitData CreateCommitData(long timestamp)
        => new(
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            ActorName.Create("rian").Value,
            Timestamp.Create(timestamp).Value,
            "Initial commit",
            [],
            [],
            [BranchName.Create("main").Value],
            IsMerge: false);

    /// <summary>Git reader test double that records clone and read calls.</summary>
    private sealed class TestGitRepositoryReader : IGitRepositoryReader
    {
        /// <summary>Commits returned by ReadCommitsAsync when ReadResult is not configured.</summary>
        public IReadOnlyList<CommitData> CommitsToReturn { get; init; } = [];

        /// <summary>Optional explicit read result.</summary>
        public Result<IReadOnlyList<CommitData>>? ReadResult { get; init; }

        /// <summary>URL passed to CloneAsync.</summary>
        public string? ClonedUrl { get; private set; }

        /// <summary>Destination path passed to CloneAsync.</summary>
        public string? CloneDestinationPath { get; private set; }

        /// <summary>Repository path passed to ReadCommitsAsync.</summary>
        public string? ReadRepositoryPath { get; private set; }

        /// <summary>Reader options passed to ReadCommitsAsync.</summary>
        public GitReaderOptions? ReadOptions { get; private set; }

        /// <summary>Records read inputs and returns configured commits or result.</summary>
        public Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
            string repositoryPath,
            GitReaderOptions options,
            CancellationToken cancellationToken = default)
        {
            ReadRepositoryPath = repositoryPath;
            ReadOptions = options;
            return Task.FromResult(ReadResult ?? Result<IReadOnlyList<CommitData>>.Success(CommitsToReturn));
        }

        /// <summary>Records clone inputs and returns success.</summary>
        public Task<Result> CloneAsync(
            string url,
            string destinationPath,
            CancellationToken cancellationToken = default)
        {
            ClonedUrl = url;
            CloneDestinationPath = destinationPath;
            return Task.FromResult(Result.Success());
        }
    }

    /// <summary>Timeline builder test double that records commits and options.</summary>
    private sealed class TestTimelineBuilder : ITimelineBuilder
    {
        /// <summary>Commits passed to Build.</summary>
        public IReadOnlyList<CommitData>? Commits { get; private set; }

        /// <summary>Options passed to Build.</summary>
        public TimelineBuilderOptions? Options { get; private set; }

        /// <summary>Timeline returned by Build.</summary>
        public Timeline TimelineToReturn { get; } = new(RepositoryId.Create("rian-be", "ChangeTrace").Value);

        /// <summary>Records inputs and returns the configured timeline.</summary>
        public Result<Timeline> Build(
            IReadOnlyList<CommitData> commits,
            TimelineBuilderOptions options)
        {
            Commits = commits;
            Options = options;
            TimelineToReturn.AddEvent(TraceEventFactory.Commit(
                Timestamp.Create(1_735_689_600).Value,
                ActorName.Create("rian").Value,
                CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value));
            return Result<Timeline>.Success(TimelineToReturn);
        }
    }

    /// <summary>Timeline repository test double that records save calls.</summary>
    private sealed class TestTimelineRepository : ITimelineRepository
    {
        /// <summary>Timeline passed to SaveAsync.</summary>
        public Timeline? SavedTimeline { get; private set; }

        /// <summary>Path passed to SaveAsync.</summary>
        public string? SavedPath { get; private set; }

        /// <summary>Result returned by SaveAsync.</summary>
        public Result SaveResult { get; init; } = Result.Success();

        /// <summary>Records save inputs and returns configured result.</summary>
        public Task<Result> SaveAsync(
            Timeline timeline,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            SavedTimeline = timeline;
            SavedPath = filePath;
            return Task.FromResult(SaveResult);
        }

        /// <summary>Load is not used by repository exporter tests.</summary>
        public Task<Result<Timeline>> LoadAsync(string filePath, CancellationToken cancellationToken = default)
            => Task.FromResult(Result<Timeline>.Failure("not used"));
    }
}
