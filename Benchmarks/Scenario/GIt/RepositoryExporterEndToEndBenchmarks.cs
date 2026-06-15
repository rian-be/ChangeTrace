using BenchmarkDotNet.Attributes;
using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using ChangeTrace.GIt.Services;
using ChangeTrace.GIt.Services.Checkpoints.Models;
using ChangeTrace.GIt.Services.Sidecars;
using Microsoft.Extensions.Logging;

namespace ChangeTrace.Benchmarks.Scenario.GIt;

/// <summary>
/// Benchmarks the end-to-end local export pipeline through
/// <see cref="RepositoryExporter.ExportAndSaveAsync"/>, including
/// commit streaming, timeline build, MsgPack serialization, and file persistence.
/// </summary>
[MemoryDiagnoser]
[InProcess]
[BenchmarkCategory(BenchmarkCategories.Git, BenchmarkCategories.Export, BenchmarkCategories.Serialization)]
public class RepositoryExporterEndToEndBenchmarks
{
    private const int CommitCount = 1_000_000;
    private const long BaseUnix = 1_700_000_000;
    private static readonly int[] FilesPerCommitPattern = [3, 5, 10, 15, 25];
    private static readonly ActorName[] Actors = CreateActors();
    private static readonly string[] Messages = CreateMessages();

    private static readonly BranchName[] Branches =
    [
        BranchName.Create("main").Value,
        BranchName.Create("develop").Value,
        BranchName.Create("feature/render").Value,
        BranchName.Create("feature/export").Value
    ];
    private static readonly IReadOnlyList<BranchName>[] BranchSets = CreateBranchSets();
    private static readonly IReadOnlyList<FileChange>[,] FileChangeSets = CreateFileChangeSets();

    private static readonly ExportOptions ExportOptions = new()
    {
        IncludeFileChanges = true,
        IncludeBranchEvents = true,
        IncludeMergeDetection = true,
        EnrichmentKinds = ExportEnrichmentKind.None
    };

    private ILoggerFactory _loggerFactory = null!;
    private RepositoryExporter _exporter = null!;
    private string _repositoryPath = null!;
    private string _outputPath = null!;

    [GlobalSetup]
    public void Setup()
    {
        _loggerFactory = LoggerFactory.Create(builder => builder.SetMinimumLevel(LogLevel.None));

        _repositoryPath = Directory.CreateTempSubdirectory("ChangeTrace.Benchmarks.Export.Source.").FullName;
        _outputPath = Path.Combine(
            Directory.CreateTempSubdirectory("ChangeTrace.Benchmarks.Export.Output.").FullName,
            "timeline");

        var serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);
        var fileManager = new FileManager();
        var repository = new TimelineRepositoryMsgPack(
            _loggerFactory.CreateLogger<TimelineRepositoryMsgPack>(),
            serializer,
            fileManager,
            new PullRequestSidecarHandler(_loggerFactory.CreateLogger<PullRequestSidecarHandler>(), fileManager),
            new MergeSidecarHandler(_loggerFactory.CreateLogger<MergeSidecarHandler>(), fileManager));

        _exporter = new RepositoryExporter(
            new GeneratedGitRepositoryReader(),
            new TimelineBuilder(_loggerFactory.CreateLogger<TimelineBuilder>()),
            repository,
            _loggerFactory.CreateLogger<RepositoryExporter>(),
            new NoOpTimelineEnricherResolver(),
            new NoOpExportCheckpointStore());
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _loggerFactory.Dispose();

        if (Directory.Exists(_repositoryPath))
            Directory.Delete(_repositoryPath, recursive: true);

        var outputDirectory = Path.GetDirectoryName(_outputPath);
        if (!string.IsNullOrEmpty(outputDirectory) && Directory.Exists(outputDirectory))
            Directory.Delete(outputDirectory, recursive: true);
    }

    [Benchmark]
    public async Task<long> ExportMillionCommitsToGitTrace()
    {
        var result = await _exporter.ExportAndSaveAsync(
            _repositoryPath,
            _outputPath,
            ExportOptions);

        if (result.IsFailure)
            throw new InvalidOperationException(result.Error);

        return new FileInfo(_outputPath + ".gittrace").Length;
    }

    private static ActorName[] CreateActors()
    {
        var actors = new ActorName[64];
        for (var index = 0; index < actors.Length; index++)
            actors[index] = ActorName.FromTrustedSerialized($"actor-{index}");

        return actors;
    }

    private static string[] CreateMessages()
    {
        var messages = new string[128];
        for (var index = 0; index < messages.Length; index++)
            messages[index] = $"Benchmark commit bucket {index}";

        return messages;
    }

    private static IReadOnlyList<BranchName>[] CreateBranchSets()
    {
        var branchSets = new IReadOnlyList<BranchName>[Branches.Length];
        for (var index = 0; index < Branches.Length; index++)
            branchSets[index] = [Branches[index]];

        return branchSets;
    }

    private static IReadOnlyList<FileChange>[,] CreateFileChangeSets()
    {
        var fileChangeSets = new IReadOnlyList<FileChange>[128, FilesPerCommitPattern.Length];

        for (var moduleIndex = 0; moduleIndex < 128; moduleIndex++)
        {
            for (var patternIndex = 0; patternIndex < FilesPerCommitPattern.Length; patternIndex++)
            {
                var filesPerCommit = FilesPerCommitPattern[patternIndex];
                var changes = new FileChange[filesPerCommit];

                for (var fileIndex = 0; fileIndex < filesPerCommit; fileIndex++)
                {
                    changes[fileIndex] = new FileChange(
                        FilePath.FromTrustedSerialized(
                            $"src/module-{moduleIndex}/feature-{fileIndex % 16}/file-slot-{fileIndex}.cs"),
                        fileIndex % 11 == 0 ? FileChangeKind.Added : FileChangeKind.Modified);
                }

                fileChangeSets[moduleIndex, patternIndex] = changes;
            }
        }

        return fileChangeSets;
    }

    private sealed class GeneratedGitRepositoryReader : IGitRepositoryReader
    {
        public Task<Result<IReadOnlyList<CommitData>>> ReadCommitsAsync(
            string repositoryPath,
            GitReaderOptions options,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result<IReadOnlyList<CommitData>>.Failure(
                "End-to-end benchmark uses streaming history only."));

        public Task<Result<IAsyncEnumerable<CommitData>>> ReadCommitsStreamAsync(
            string repositoryPath,
            GitReaderOptions options,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Result<IAsyncEnumerable<CommitData>>.Success(
                StreamCommits(cancellationToken)));
        }

        public Task<Result> CloneAsync(
            string url,
            string destinationPath,
            CancellationToken cancellationToken = default)
            => Task.FromResult(Result.Success());

        private static async IAsyncEnumerable<CommitData> StreamCommits(
            [System.Runtime.CompilerServices.EnumeratorCancellation] CancellationToken cancellationToken)
        {
            for (var index = 0; index < CommitCount; index++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new CommitData(
                    Sha: CommitSha.FromTrustedSerialized(index.ToString("x40")),
                    Author: Actors[index % Actors.Length],
                    Timestamp: Timestamp.FromTrustedUnixSeconds(BaseUnix + index),
                    Message: Messages[index % Messages.Length],
                    ParentShas: CreateParentShas(index),
                    FileChanges: CreateFileChanges(index),
                    Branches: BranchSets[index % BranchSets.Length],
                    IsMerge: index > 0 && index % 50 == 0);
            }
        }

        private static IReadOnlyList<CommitSha> CreateParentShas(int index)
        {
            if (index == 0)
                return [];

            if (index > 1 && index % 50 == 0)
            {
                return
                [
                    CommitSha.FromTrustedSerialized((index - 1).ToString("x40")),
                    CommitSha.FromTrustedSerialized(Math.Max(0, index - 17).ToString("x40"))
                ];
            }

            return
            [
                CommitSha.FromTrustedSerialized((index - 1).ToString("x40"))
            ];
        }

        private static IReadOnlyList<FileChange> CreateFileChanges(int commitIndex)
        {
            var moduleIndex = commitIndex % 128;
            var filesPerCommit = FilesPerCommitPattern[commitIndex % FilesPerCommitPattern.Length];
            var patternIndex = Array.IndexOf(FilesPerCommitPattern, filesPerCommit);
            return FileChangeSets[moduleIndex, patternIndex];
        }
    }

    private sealed class NoOpTimelineEnricherResolver : ITimelineEnricherResolver
    {
        public bool TryResolve(string provider, out ITimelineEnricher? enricher)
        {
            enricher = null;
            return false;
        }
    }

    private sealed class NoOpExportCheckpointStore : IExportCheckpointStore
    {
        public Task<ExportCheckpointState?> TryLoad(
            string checkpointKey,
            string expectedFingerprint,
            CancellationToken cancellationToken = default)
            => Task.FromResult<ExportCheckpointState?>(null);

        public Task Save(
            string checkpointKey,
            ExportCheckpointState state,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task AppendPullRequestPatch(
            string checkpointKey,
            ExportCheckpointState state,
            int targetIndex,
            ChangeTrace.Core.Events.TraceEvent updatedEvent,
            CancellationToken cancellationToken = default)
            => Task.CompletedTask;

        public Task Clear(string checkpointKey, CancellationToken cancellationToken = default)
            => Task.CompletedTask;
    }
}
