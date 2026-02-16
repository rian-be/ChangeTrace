using ChangeTrace.Core;
using ChangeTrace.Core.Events;
using Model = ChangeTrace.Core.Models;

namespace ChangeTrace.GIt.Dto;

/// <summary>
/// Data Transfer Object representing <see cref="Timeline"/>.
/// </summary>
/// <remarks>
/// Captures the timeline's name, repository identity, and the list of <see cref="TraceEventDto"/> events.
/// Also tracks whether the timeline has been normalized.
/// 
/// Provides conversion methods <see cref="FromDomain"/> and <see cref="ToDomain"/> for mapping between domain and DTO.
/// </remarks>
internal sealed record TimelineDto
{
    internal string? Name { get; init; }
    internal RepositoryIdDto? RepositoryId { get; init; }
    internal List<TraceEventDto> Events { get; init; } = [];
    internal bool IsNormalized { get; init; }

    internal static TimelineDto FromDomain(Timeline timeline)
    {
        return new TimelineDto
        {
            Name = timeline.Name,
            RepositoryId = timeline.RepositoryId != null
                ? new RepositoryIdDto(timeline.RepositoryId.Owner, timeline.RepositoryId.Name)
                : null,
            Events = timeline.Events.Select(TraceEventDto.FromDomain).ToList(),
            IsNormalized = timeline.IsNormalized
        };
    }

    internal Timeline ToDomain()
    {
        var repositoryId = RepositoryId != null
            ? Model.RepositoryId.Create(RepositoryId.Owner, RepositoryId.Name).ValueOrNull
            : null;

        var timeline = new Timeline(Name, repositoryId);

        foreach (var evt in Events.Select(eventDto => eventDto.ToDomain()).OfType<TraceEvent>())
        {
            timeline.AddEvent(evt);
        }

        if (IsNormalized)
        {
            timeline.Normalize();
        }

        return timeline;
    }
}