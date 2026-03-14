using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters.Commit;

/// <summary>
/// Specification that matches trace events associated with a file path.
/// Used to filter events that represent file-level changes.
/// </summary>
internal sealed class HasFilePathSpec : Specification<TraceEvent>
{
    internal override bool IsSatisfiedBy(TraceEvent candidate)
        => candidate.Metadata is not { FilePath: null };
}