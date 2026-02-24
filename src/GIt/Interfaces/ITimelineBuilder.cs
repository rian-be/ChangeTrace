using ChangeTrace.Core;
using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;

namespace ChangeTrace.GIt.Interfaces;

internal interface ITimelineBuilder
{
    Result<Timeline> Build(
        IReadOnlyList<CommitData> commits,
        TimelineBuilderOptions options);
}