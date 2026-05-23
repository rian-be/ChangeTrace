using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Rendering.Enums;
using ChangeTrace.Rendering.Pipeline;

namespace ChangeTrace.Rendering.Processors;

internal static class RenderEventDispatchTable
{
    internal delegate void DispatchFn(
        RenderingPipeline pipeline,
        TraceEventAggregationStage aggregation);

    internal static readonly (RenderEventKinds Kind, DispatchFn Fn)[] Table =
    {
        (RenderEventKinds.Commit, DispatchCommit),
        (RenderEventKinds.Branch, DispatchBranch),
        (RenderEventKinds.Merge, DispatchMerge),
        (RenderEventKinds.FileCoupling, DispatchFileCoupling)
    };

    private static void DispatchCommit(
        RenderingPipeline pipeline,
        TraceEventAggregationStage aggregation) =>
        pipeline.DispatchAggregated(
            aggregation.GetWriter<CommitBundleEvent>());

    private static void DispatchBranch(
        RenderingPipeline pipeline,
        TraceEventAggregationStage aggregation) =>
        pipeline.DispatchAggregated(
            aggregation.GetWriter<BranchEvent>());

    private static void DispatchMerge(
        RenderingPipeline pipeline,
        TraceEventAggregationStage aggregation) =>
        pipeline.DispatchAggregated(
            aggregation.GetWriter<MergeEvent>());

    private static void DispatchFileCoupling(
        RenderingPipeline pipeline,
        TraceEventAggregationStage aggregation) =>
        pipeline.DispatchAggregated(
            aggregation.GetWriter<FileCouplingEvent>());
}