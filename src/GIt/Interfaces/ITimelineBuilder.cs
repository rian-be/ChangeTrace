using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;
using ChangeTrace.Core.Timelines;

namespace ChangeTrace.GIt.Interfaces;

internal interface ITimelineBuilder
{
    Task<Result<Timeline>> Build(
        IAsyncEnumerable<CommitData> commits,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken = default);
}
