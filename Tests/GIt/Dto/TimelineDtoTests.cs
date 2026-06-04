using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto;
using Xunit;

namespace ChangeTrace.Tests.GIt.Dto;

/// <summary>Tests Git timeline DTO conversion to and from domain timelines.</summary>
public sealed class TimelineDtoTests
{
    /// <summary>FromDomain preserves repository identity, event data, and normalized ordering state.</summary>
    [Fact]
    public void FromDomain_CopiesRepositoryEventsAndNormalizedState()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreateCommit(100));
        timeline.AddEvent(CreateFileChange(200));

        var dto = TimelineDto.FromDomain(timeline);

        Assert.NotNull(dto.RepositoryId);
        Assert.Equal("rian-be", dto.RepositoryId.Owner);
        Assert.Equal("ChangeTrace", dto.RepositoryId.Name);
        Assert.True(dto.IsNormalized);
        Assert.Equal(2, dto.Events.Count);
        Assert.Equal("0123456789abcdef0123456789abcdef01234567", dto.Events[1].CommitSha);
        Assert.Equal("src/GIt/Services/GitRepositoryReader.cs", dto.Events[1].FilePath);
        Assert.Equal(FileChangeKind.Modified.ToString(), dto.Events[1].CommitType);
    }

    /// <summary>FromDomain marks a DTO as not normalized when event timestamps move backward.</summary>
    [Fact]
    public void FromDomain_MarksTimelineAsNotNormalizedWhenEventsAreOutOfOrder()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(CreateCommit(200));
        timeline.AddEvent(CreateCommit(100));

        var dto = TimelineDto.FromDomain(timeline);

        Assert.False(dto.IsNormalized);
    }

    /// <summary>ToDomain rebuilds repository identity and supported event shapes from DTO data.</summary>
    [Fact]
    public void ToDomain_RebuildsRepositoryAndSupportedEvents()
    {
        var dto = new TimelineDto
        {
            RepositoryId = new RepositoryIdDto("rian-be", "ChangeTrace"),
            Events =
            [
                TraceEventDto.FromDomain(CreateCommit(100)),
                TraceEventDto.FromDomain(CreateFileChange(200)),
                TraceEventDto.FromDomain(CreateBranch(300)),
                TraceEventDto.FromDomain(CreateMerge(400))
            ],
            IsNormalized = true
        };

        var timeline = dto.ToDomain();

        Assert.Equal("rian-be", timeline.RepositoryId?.Owner);
        Assert.Equal("ChangeTrace", timeline.RepositoryId?.Name);
        Assert.Equal(4, timeline.Events.Count);
        Assert.Equal("Initial commit", timeline.Events[0].Metadata?.Metadata);
        Assert.Equal("src/GIt/Services/GitRepositoryReader.cs", timeline.Events[1].Metadata?.FilePath?.Value);
        Assert.Equal(BranchEventType.BranchCreated, timeline.Events[2].Branch?.Type);
        Assert.Equal(BranchEventType.Merge, timeline.Events[3].Branch?.Type);
    }

    /// <summary>ToDomain skips events that cannot be rebuilt from invalid DTO values.</summary>
    [Fact]
    public void ToDomain_SkipsInvalidEvents()
    {
        var dto = new TimelineDto
        {
            Events =
            [
                new TraceEventDto
                {
                    Timestamp = 100,
                    Actor = "rian",
                    CommitSha = "not-a-sha"
                },
                TraceEventDto.FromDomain(CreateCommit(200))
            ]
        };

        var timeline = dto.ToDomain();

        var evt = Assert.Single(timeline.Events);
        Assert.Equal(200, evt.Core.Timestamp.UnixSeconds);
    }

    /// <summary>Creates commit event for DTO conversion tests.</summary>
    private static TraceEvent CreateCommit(long timestamp)
        => TraceEventFactory.Commit(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Initial commit");

    /// <summary>Creates file change event for DTO conversion tests.</summary>
    private static TraceEvent CreateFileChange(long timestamp)
        => TraceEventFactory.FileChange(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            FilePath.Create("src/GIt/Services/GitRepositoryReader.cs").Value,
            FileChangeKind.Modified,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Update reader");

    /// <summary>Creates branch event for DTO conversion tests.</summary>
    private static TraceEvent CreateBranch(long timestamp)
        => TraceEventFactory.Branch(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            BranchName.Create("feature/git-tests").Value,
            BranchEventType.BranchCreated,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Create branch");

    /// <summary>Creates merge event for DTO conversion tests.</summary>
    private static TraceEvent CreateMerge(long timestamp)
        => TraceEventFactory.Merge(
            Timestamp.Create(timestamp).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            BranchName.Create("main").Value,
            "Merge feature");
}
