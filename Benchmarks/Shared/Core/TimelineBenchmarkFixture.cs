using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;

namespace ChangeTrace.Benchmarks.Shared.Core;

/// <summary>
/// Shared deterministic input data for timeline, serialization, and aggregation benchmarks.
/// </summary>
internal sealed class TimelineBenchmarkFixture
{
    private TimelineBenchmarkFixture(
        int commitCount,
        int filesPerCommit,
        IReadOnlyList<CommitData> commits,
        Timeline timeline,
        TraceEvent[] traceEvents)
    {
        CommitCount = commitCount;
        FilesPerCommit = filesPerCommit;
        Commits = commits;
        Timeline = timeline;
        TraceEvents = traceEvents;
    }

    public int CommitCount { get; }

    public int FilesPerCommit { get; }

    public IReadOnlyList<CommitData> Commits { get; }

    public Timeline Timeline { get; }

    public TraceEvent[] TraceEvents { get; }

    public static RepositoryId RepositoryId { get; } =
        RepositoryId.Create("bench", "timeline").Value;

    public static TimelineBenchmarkFixture Create(
        int commitCount,
        int filesPerCommit = 4,
        bool includeMerges = true)
    {
        var commits = CreateCommits(commitCount, filesPerCommit, includeMerges);
        var timeline = CreateTimeline(commits);

        return new TimelineBenchmarkFixture(
            commitCount,
            filesPerCommit,
            commits,
            timeline,
            [.. timeline.Events]);
    }

    public static IReadOnlyList<CommitData> CreateCommits(
        int commitCount,
        int filesPerCommit = 4,
        bool includeMerges = true)
    {
        var commits = new CommitData[commitCount];
        var branches = new[]
        {
            BranchName.Create("main").Value,
            BranchName.Create("develop").Value,
            BranchName.Create("feature/render").Value,
            BranchName.Create("feature/export").Value
        };

        const long baseUnix = 1_700_000_000;

        for (var i = 0; i < commitCount; i++)
        {
            var sha = CreateSha(i);
            var parentShas = CreateParentShas(i, includeMerges);
            var fileChanges = CreateFileChanges(i, filesPerCommit);
            var commitBranches = new[]
            {
                branches[i % branches.Length]
            };

            commits[i] = new CommitData(
                Sha: sha,
                Author: ActorName.Create($"actor-{i % 64}").Value,
                Timestamp: Timestamp.Create(baseUnix + i).Value,
                Message: $"Benchmark commit {i}",
                ParentShas: parentShas,
                FileChanges: fileChanges,
                Branches: commitBranches,
                IsMerge: includeMerges && i > 0 && i % 50 == 0);
        }

        return commits;
    }

    public static Timeline CreateTimeline(
        IReadOnlyList<CommitData> commits,
        bool includeFileChanges = true,
        bool includeBranchEvents = true,
        bool includeMergeEvents = true)
    {
        var timeline = new Timeline(RepositoryId);

        foreach (var commit in commits)
        {
            timeline.AddEvent(TraceEventFactory.Commit(
                commit.Timestamp,
                commit.Author,
                commit.Sha,
                commit.Message));

            if (includeFileChanges)
            {
                foreach (var change in commit.FileChanges)
                {
                    timeline.AddEvent(TraceEventFactory.FileChange(
                        commit.Timestamp,
                        commit.Author,
                        change.Path,
                        change.Kind,
                        commit.Sha,
                        change.OldPath?.Value));
                }
            }

            if (includeBranchEvents)
            {
                foreach (var branch in commit.Branches)
                {
                    timeline.AddEvent(TraceEventFactory.Branch(
                        commit.Timestamp,
                        commit.Author,
                        branch,
                        BranchEventType.BranchCreated,
                        commit.Sha));
                }
            }

            if (includeMergeEvents && commit.IsMerge)
            {
                timeline.AddEvent(TraceEventFactory.Merge(
                    commit.Timestamp,
                    commit.Author,
                    commit.Sha,
                    commit.Branches[0],
                    commit.Message));
            }
        }

        return timeline;
    }

    public static Timeline CloneTimeline(Timeline source)
    {
        var timeline = new Timeline(source.RepositoryId);
        timeline.AddEvents(source.Events);
        return timeline;
    }

    public static CommitSha CreateSha(int value)
        => CommitSha.Create(value.ToString("x40")).Value;

    private static IReadOnlyList<CommitSha> CreateParentShas(int index, bool includeMerges)
    {
        if (index == 0)
            return [];

        if (includeMerges && index > 1 && index % 50 == 0)
        {
            return
            [
                CreateSha(index - 1),
                CreateSha(Math.Max(0, index - 17))
            ];
        }

        return [CreateSha(index - 1)];
    }

    private static IReadOnlyList<FileChange> CreateFileChanges(
        int commitIndex,
        int filesPerCommit)
    {
        var changes = new FileChange[filesPerCommit];

        for (var fileIndex = 0; fileIndex < filesPerCommit; fileIndex++)
        {
            var path = FilePath.Create(
                $"src/module-{commitIndex % 128}/feature-{fileIndex % 16}/file-{commitIndex}-{fileIndex}.cs").Value;

            changes[fileIndex] = new FileChange(
                path,
                fileIndex % 11 == 0 ? FileChangeKind.Added : FileChangeKind.Modified);
        }

        return changes;
    }
}
