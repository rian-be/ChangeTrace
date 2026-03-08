using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Timelines;

/// <summary>
/// Represents an ordered collection of <see cref="TraceEvent"/> instances
/// describing the evolution of a repository over time.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Stores events in chronological order.</item>
/// <item>Provides efficient access through both <see cref="Events"/> and <see cref="EventsSpan"/>.</item>
/// <item>Acts as the primary container for timeline-based analysis and playback.</item>
/// </list>
/// </remarks>
internal sealed class Timeline(string? name = null, RepositoryId? repositoryId = null)
{
    private readonly List<TraceEvent> _events = [];

    /// <summary>
    /// Gets the list of events in the timeline.
    /// </summary>
    internal IReadOnlyList<TraceEvent> Events => _events;

    /// <summary>
    /// Gets a span view over the underlying events collection.
    /// </summary>
    /// <remarks>
    /// This method avoids allocations and is intended for high-performance iteration scenarios.
    /// </remarks>
    internal ReadOnlySpan<TraceEvent> EventsSpan
        => System.Runtime.InteropServices.CollectionsMarshal.AsSpan(_events);

    /// <summary>
    /// Gets the number of events stored in the timeline.
    /// </summary>
    internal int Count => _events.Count;

    /// <summary>
    /// Gets the optional human-readable name of the timeline.
    /// </summary>
    internal string? Name { get; } = name;

    /// <summary>
    /// Gets the identifier of the repository associated with this timeline.
    /// </summary>
    internal RepositoryId? RepositoryId { get; } = repositoryId;

    /// <summary>
    /// Finds the first event that matches the specified predicate.
    /// </summary>
    /// <param name="predicate">Predicate used to test events.</param>
    /// <returns>The first matching event or <c>null</c> if none match.</returns>
    internal TraceEvent? FindFirst(Func<TraceEvent, bool> predicate)
        => _events.FirstOrDefault(predicate);

    /// <summary>
    /// Adds a single event to the timeline.
    /// </summary>
    /// <param name="evt">Event to add.</param>
    internal void AddEvent(TraceEvent evt) =>
        _events.Add(evt);

    /// <summary>
    /// Adds multiple events to the timeline.
    /// </summary>
    /// <param name="events">Events to add.</param>
    internal void AddEvents(IEnumerable<TraceEvent> events) =>
        _events.AddRange(events);
    
    internal void Clear() =>
        _events.Clear();
}