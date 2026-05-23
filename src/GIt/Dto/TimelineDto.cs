using ChangeTrace.Core.Timelines;
using MessagePack;

using Model = ChangeTrace.Core.Models;

namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Data Transfer Object representing <see cref="Timeline"/>.
/// </summary>
[MessagePackObject(AllowPrivate = true)]
internal sealed record TimelineDto
{
    [Key(1)] internal RepositoryIdDto? RepositoryId { get; init; }

    [Key(2)] internal required List<TraceEventDto> Events { get; init; }
    [Key(3)] internal bool IsNormalized { get; init; }

    internal static TimelineDto FromDomain(Timeline timeline)
    {
        return new TimelineDto
        {
            RepositoryId = timeline.RepositoryId is not null
                ? new RepositoryIdDto(
                    timeline.RepositoryId.Owner,
                    timeline.RepositoryId.Name)
                : null,

            Events = timeline.Events
                .Select(TraceEventDto.FromDomain)
                .ToList(),

            IsNormalized = IsTimelineNormalized(timeline)
        };
    }

    internal Timeline ToDomain()
    {
        var repositoryId = RepositoryId is not null
            ? Model.RepositoryId.Create(
                RepositoryId.Owner,
                RepositoryId.Name).ValueOrNull
            : null;

        var timeline = new Timeline(repositoryId);

        foreach (var eventDto in Events)
        {
            if (eventDto.ToDomain() is { } traceEvent)
                timeline.AddEvent(traceEvent);
        }

        return timeline;
    }

    private static bool IsTimelineNormalized(Timeline timeline)
    {
        var events = timeline.EventsSpan;

        if (events.Length <= 1)
            return true;

        var previousTimestamp = events[0].Core.Timestamp;

        for (var i = 1; i < events.Length; i++)
        {
            var currentTimestamp = events[i].Core.Timestamp;

            if (currentTimestamp < previousTimestamp)
                return false;

            previousTimestamp = currentTimestamp;
        }

        return true;
    }
}
