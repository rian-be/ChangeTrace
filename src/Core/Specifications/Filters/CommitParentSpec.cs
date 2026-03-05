using ChangeTrace.Core.Events;

namespace ChangeTrace.Core.Specifications.Filters;

internal sealed class CommitParentSpec(string commitSha) : Specification<TraceEvent>
{
    internal override bool IsSatisfiedBy(TraceEvent item)
        => item.CommitSha != null
           && item.CommitSha.Value == commitSha
           && item.Target == commitSha;
}