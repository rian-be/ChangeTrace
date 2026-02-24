using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Results;

namespace ChangeTrace.Core;

/// <summary>
/// Timeline aggregate - manages collection of events.
/// Rich model with behavior and validation.
/// </summary>
internal sealed class Timeline(string? name = null, RepositoryId? repositoryId = null)
{
    private readonly List<TraceEvent> _events = [];
    private bool _isNormalized;

    internal IReadOnlyList<TraceEvent> Events => _events;
    internal bool IsNormalized => _isNormalized;
    internal int Count => _events.Count;
    internal string? Name { get; } = name;
    internal RepositoryId? RepositoryId { get; } = repositoryId;

    /// <summary>
    /// Add single event
    /// </summary>
    internal void AddEvent(TraceEvent evt)
    {
        _events.Add(evt);
        _isNormalized = false;
        Result.Success();
    }

    /// <summary>
    /// Add multiple events efficiently
    /// </summary>
    internal Result AddEvents(IEnumerable<TraceEvent> events)
    {
        var list = events.ToList();
        if (list.Count == 0)
            return Result.Success();

        _events.AddRange(list);
        _isNormalized = false;
        return Result.Success();
    }

    /// <summary>
    /// Normalize all timestamps relative to first event
    /// </summary>
    internal Result Normalize(double targetDurationSeconds = 300.0)
    {
        if (_events.Count == 0)
            return Result.Failure("Cannot normalize empty timeline");

        _events.Sort((a, b) => a.Timestamp.CompareTo(b.Timestamp));

        var baseTime     = _events[0].Timestamp;
        var originalSpan = _events[^1].Timestamp.UnixSeconds - baseTime.UnixSeconds;
        var scale        = originalSpan > 1e-9 ? targetDurationSeconds / originalSpan : 1.0;

        foreach (var evt in _events)
            evt.NormalizeTime(baseTime, scale);

        _isNormalized = true;
        return Result.Success();
    }

    /// <summary>
    /// Get timeline statistics
    /// </summary>
    internal TimelineStatistics GetStatistics()
    {
        if (_events.Count == 0)
            return TimelineStatistics.Empty;

        return new TimelineStatistics(
            TotalEvents: _events.Count,
            CommitCount: _events.Count(e => e.CommitType.HasValue),
            BranchCount: _events.Count(e => e.BranchType.HasValue),
            PullRequestCount: _events.Count(e => e.PrType.HasValue),
            MergeCommitCount: _events.Count(e => e.IsMergeCommit()),
            UniqueActors: _events.Select(e => e.Actor).Distinct().Count(),
            TimeSpanSeconds: _events.Max(e => e.Timestamp).UnixSeconds - _events.Min(e => e.Timestamp).UnixSeconds,
            StartTime: _events.Min(e => e.Timestamp),
            EndTime: _events.Max(e => e.Timestamp)
        );
    }

    /// <summary>
    /// Find first event matching predicate
    /// </summary>
    internal TraceEvent? FindFirst(Func<TraceEvent, bool> predicate)
    {
        return _events.FirstOrDefault(predicate);
    }

    /// <summary>
    /// Query events with predicate
    /// </summary>
    internal IEnumerable<TraceEvent> Query(Func<TraceEvent, bool> predicate)
    {
        return _events.Where(predicate);
    }

    /// <summary>
    /// Get all commits
    /// </summary>
    internal IEnumerable<TraceEvent> GetCommits() 
        => _events.Where(e => e.CommitType.HasValue);

    /// <summary>
    /// Get all branches
    /// </summary>
    internal IEnumerable<TraceEvent> GetBranches() 
        => _events.Where(e => e.BranchType.HasValue);

    /// <summary>
    /// Get all pull requests
    /// </summary>
    internal IEnumerable<TraceEvent> GetPullRequests() 
        => _events.Where(e => e.PrType.HasValue);

    /// <summary>
    /// Get all merge commits
    /// </summary>
    internal IEnumerable<TraceEvent> GetMergeCommits() 
        => _events.Where(e => e.IsMergeCommit());

    /// <summary>
    /// Get events by actor
    /// </summary>
    internal IEnumerable<TraceEvent> GetByActor(ActorName actor) 
        => _events.Where(e => e.Actor == actor);

    /// <summary>
    /// Get events in time range
    /// </summary>
    internal IEnumerable<TraceEvent> GetByTimeRange(Timestamp start, Timestamp end)
        => _events.Where(e => e.Timestamp >= start && e.Timestamp <= end);

    /// <summary>
    /// Get unique actors
    /// </summary>
    internal IEnumerable<ActorName> GetUniqueActors()
        => _events.Select(e => e.Actor).Distinct();

    /// <summary>
    /// Clear all events (for testing)
    /// </summary>
    internal void Clear()
    {
        _events.Clear();
        _isNormalized = false;
    }
}