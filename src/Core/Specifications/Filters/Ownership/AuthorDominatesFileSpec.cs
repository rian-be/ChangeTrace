using ChangeTrace.Core.Events;
using ChangeTrace.Core.Models;

namespace ChangeTrace.Core.Specifications.Filters.Ownership;

/// <summary>
/// Matches events where a single author has at least `minCommits` in a given file.
/// Requires full timeline of events for evaluation.
/// </summary>
internal sealed class AuthorDominatesFileSpec : Specification<TraceEvent>
{
    private readonly HashSet<ActorName> _dominantAuthors;
    private readonly FilePath _filePath;

    internal AuthorDominatesFileSpec(IEnumerable<TraceEvent> allEvents, FilePath filePath, int minCommits)
    {
        _filePath = filePath;
        _dominantAuthors = allEvents
            .Where(e => e.FilePath != null && e.FilePath.Equals(filePath))
            .GroupBy(e => e.Actor)
            .Where(g => g.Count() >= minCommits)
            .Select(g => g.Key)
            .ToHashSet();
    }

    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.FilePath != null 
           && item.FilePath.Equals(_filePath) 
           && _dominantAuthors.Contains(item.Actor);
}