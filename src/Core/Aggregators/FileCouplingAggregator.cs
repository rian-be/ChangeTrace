using ChangeTrace.Core.Events.Semantic;
using ChangeTrace.Core.Interfaces;

namespace ChangeTrace.Core.Aggregators;

/// <summary>
/// Aggregates commit bundle events into file coupling events.
/// </summary>
/// <remarks>
/// <para>
/// For each <see cref="CommitBundleEvent"/>, this aggregator generates
/// <see cref="FileCouplingEvent"/> instances for every unique pair of files
/// modified together in the commit.
/// </para>
/// <para>
/// This is used for building file dependency graphs and detecting
/// architectural coupling between files in repository over time.
/// </para>
/// </remarks>
internal sealed class FileCouplingAggregator(SemanticEventWriter<FileCouplingEvent> writer)
    : IEventAggregator<CommitBundleEvent>
{
    /// <summary>
    /// Processes single <see cref="CommitBundleEvent"/> and emits
    /// <see cref="FileCouplingEvent"/> for all file pairs modified together.
    /// </summary>
    /// <param name="bundle">
    /// The commit bundle containing the list of changed files and the commit timestamp.
    /// </param>
    /// <remarks>
    /// <para>
    /// If bundle contains fewer than two files, no events are emitted.
    /// Otherwise,  <see cref="FileCouplingEvent"/> is created for every
    /// distinct pair of files in bundle.
    /// </para>
    /// <para>
    /// Events are written to provided <see cref="SemanticEventWriter{FileCouplingEvent}"/>.
    /// </para>
    /// </remarks>
    public void Process(CommitBundleEvent bundle)
    {
        var files = bundle.Files.Span;

        if (files.Length < 2)
            return;

        var timestamp = bundle.Timestamp;

        for (var i = 0; i < files.Length; i++)
        {
            var fileA = files[i];

            for (var j = i + 1; j < files.Length; j++)
            {
                var fileB = files[j];

                writer.Write(
                    new FileCouplingEvent(
                        timestamp,
                        fileA,
                        fileB
                    ));
            }
        }
    }
    
    public void Flush()
    {
        // nothing buffered
    }
}