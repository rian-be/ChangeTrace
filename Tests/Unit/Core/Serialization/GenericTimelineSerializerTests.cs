using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Services;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Dto;
using ChangeTrace.GIt.Services;
using MessagePack;
using MessagePack.Resolvers;
using Xunit;

namespace ChangeTrace.Tests.Core.Serialization;

/// <summary>Tests generic timeline serialization with current and legacy payloads.</summary>
public sealed class GenericTimelineSerializerTests
{
    /// <summary>RoundTrip serializes and deserializes a timeline through the generic serializer.</summary>
    [Fact]
    public async Task RoundTrip_UsesGenericTimelineSerializer()
    {
        var serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);

        var timeline = CreateTimeline();

        var bytes = await serializer.SerializeAsync(timeline);
        var deserialized = await serializer.DeserializeAsync(bytes);

        AssertTimeline(deserialized);
    }

    /// <summary>DeserializeAsync reads the legacy compressed TimelineDto payload format.</summary>
    [Fact]
    public async Task DeserializeAsync_ReadsLegacyCompressedTimelineDtoPayload()
    {
        var legacyOptions = MessagePackSerializerOptions.Standard
            .WithCompression(MessagePackCompression.Lz4BlockArray)
            .WithResolver(StandardResolverAllowPrivate.Instance);
        var legacyBytes = MessagePackSerializer.Serialize(
            TimelineDto.FromDomain(CreateTimeline()),
            legacyOptions);
        var serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);

        var deserialized = await serializer.DeserializeAsync(legacyBytes);

        AssertTimeline(deserialized);
    }

    /// <summary>RoundTrip preserves pull request metadata attached to a timeline event.</summary>
    [Fact]
    public async Task RoundTrip_PreservesPullRequestMetadata()
    {
        var serializer = new MessagePackSerializer<Timeline>(
            [new TimelineMessagePackFormatter()]);

        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        var commit = TraceEventFactory.Commit(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Update timeline")
            .WithPullRequest(
                PullRequestNumber.Create(27).Value,
                PullRequestEventType.PullRequestMerged);
        timeline.AddEvent(commit);

        var bytes = await serializer.SerializeAsync(timeline);
        var deserialized = await serializer.DeserializeAsync(bytes);

        Assert.Equal(27, deserialized.Events[0].PullRequest?.Number.Value);
        Assert.Equal(PullRequestEventType.PullRequestMerged, deserialized.Events[0].PullRequest?.Type);
    }

    /// <summary>Creates a timeline containing one file-change event for serializer assertions.</summary>
    private static Timeline CreateTimeline()
    {
        var timeline = new Timeline(RepositoryId.Create("rian-be", "ChangeTrace").Value);
        timeline.AddEvent(TraceEventFactory.FileChange(
            Timestamp.Create(1_735_689_600).Value,
            ActorName.Create("rian").Value,
            FilePath.Create("src/Core/Timelines/TImeline.cs").Value,
            FileChangeKind.Modified,
            CommitSha.Create("0123456789abcdef0123456789abcdef01234567").Value,
            "Update timeline"));
        return timeline;
    }

    /// <summary>Asserts the repository and event fields preserved by serialization.</summary>
    private static void AssertTimeline(Timeline deserialized)
    {
        Assert.Equal("rian-be", deserialized.RepositoryId?.Owner);
        Assert.Equal("ChangeTrace", deserialized.RepositoryId?.Name);
        Assert.Single(deserialized.Events);
        Assert.Equal("rian", deserialized.Events[0].Core.Actor.Value);
        Assert.Equal("src/Core/Timelines/TImeline.cs", deserialized.Events[0].Metadata?.FilePath?.Value);
    }
}
