using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Enums;

namespace ChangeTrace.Rendering.Processors;

/// <summary>
/// Provides static dispatch table mapping <see cref="RenderEventKinds"/> to corresponding
/// rendering dispatch functions.
/// </summary>
/// <remarks>
/// <para>
/// Each entry in the table associates kind of render event (commit, branch, merge, file coupling)
/// with delegate that knows how to dispatch aggregated events of that type through <see cref="RenderingPipeline"/>.
/// </para>
/// <para>
/// This allows the rendering system to efficiently invoke the correct dispatch logic
/// for each type of event without runtime type checks.
/// </para>
/// </remarks>
internal static class RenderEventDispatchTable
{
    /// <summary>
    /// Delegate type for dispatch functions.
    /// </summary>
    /// <param name="pipeline">The rendering pipeline to use for dispatch.</param>
    internal delegate void DispatchFn(RenderingPipeline pipeline);

    /// <summary>
    /// Static table mapping event kinds to dispatch functions.
    /// </summary>
    internal static readonly (RenderEventKinds Kind, DispatchFn Fn)[] Table =
    {
        (RenderEventKinds.Commit, DispatchCommit),
        (RenderEventKinds.Branch, DispatchBranch),
        (RenderEventKinds.Merge, DispatchMerge),
        (RenderEventKinds.FileCoupling, DispatchFileCoupling)
    };

    /// <summary>
    /// Dispatches commit events through the rendering pipeline.
    /// </summary>
    /// <param name="p">The rendering pipeline instance.</param>
    private static void DispatchCommit(RenderingPipeline p) =>
        p.DispatchAggregated(p.Aggregation.GetWriter<CommitBundleEvent>());

    /// <summary>
    /// Dispatches branch events through the rendering pipeline.
    /// </summary>
    /// <param name="p">The rendering pipeline instance.</param>
    private static void DispatchBranch(RenderingPipeline p) =>
        p.DispatchAggregated(p.Aggregation.GetWriter<BranchEvent>());

    /// <summary>
    /// Dispatches merge events through the rendering pipeline.
    /// </summary>
    /// <param name="p">The rendering pipeline instance.</param>
    private static void DispatchMerge(RenderingPipeline p) =>
        p.DispatchAggregated(p.Aggregation.GetWriter<MergeEvent>());

    /// <summary>
    /// Dispatches file coupling events through the rendering pipeline.
    /// </summary>
    /// <param name="p">The rendering pipeline instance.</param>
    private static void DispatchFileCoupling(RenderingPipeline p) =>
        p.DispatchAggregated(p.Aggregation.GetWriter<FileCouplingEvent>());
}