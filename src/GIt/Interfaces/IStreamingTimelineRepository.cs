using ChangeTrace.Core.Models;
using ChangeTrace.Core.Options;
using ChangeTrace.Core.Results;

namespace ChangeTrace.GIt.Interfaces;

/// <summary>
/// Optional repository extension that can persist a timeline directly from a commit stream.
/// </summary>
internal interface IStreamingTimelineRepository
{
    /// <summary>
    /// Persists timeline events directly from the source commit stream without
    /// first materializing a full <see cref="ChangeTrace.Core.Timelines.Timeline"/>.
    /// </summary>
    Task<Result> SaveAsync(
        IAsyncEnumerable<CommitData> commits,
        string filePath,
        TimelineBuilderOptions options,
        CancellationToken cancellationToken = default);
}
