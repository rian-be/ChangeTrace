using ChangeTrace.Core.Enums;
using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters.Commit;

/// <summary>
/// Specification that matches commits which are not merge commits.
/// </summary>
internal sealed class NonMergeCommitSpec : Specification<TraceEvent>
{
    internal override bool IsSatisfiedBy(TraceEvent candidate)
        => candidate.Branch is not { Type: BranchEventType.Merge };
}