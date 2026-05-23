using System.CommandLine;
using ChangeTrace.Cli.Commands.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Player.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Debug.Player;

/// <summary>
/// Handles debug playback of serialized timelines.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(PlayerDebugCommandHandler))]
internal sealed class PlayerDebugCommandHandler(
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory) : ICliHandler
{
    /// <summary>
    /// Loads, normalizes, summarizes, and replays a timeline for diagnostics.
    /// </summary>
    public async Task HandleAsync(
        ParseResult parseResult,
        CancellationToken cancellationToken)
    {
        var filePath = parseResult.GetValue<string>("file")!;

        var timeline = await TimelineLoader.LoadAsync(
            serializer,
            filePath,
            cancellationToken);

        if (timeline == null)
            return;

        var normalizeResult = TimelineNormalizer.Normalize(timeline);

        if (!normalizeResult.IsSuccess)
        {
            Console.WriteLine($"Normalization failed: {normalizeResult.Error}");
            return;
        }

        Console.WriteLine($"Timeline: {timeline.Events.Count} events");
        Console.WriteLine($"Repository: {timeline.RepositoryId?.Owner}/{timeline.RepositoryId?.Name}\n");

        var aggregation = PlayerDebugAggregation.Build(timeline);
        aggregation.PrintSummary();

        var renderer = new PlayerDebugRenderer();

        var session = new PlayerDebugSession(
            playerFactory,
            timeline,
            aggregation,
            renderer);

        await session.RunAsync(cancellationToken);

        Console.WriteLine("\n=== Playback Finished ===");
        PlayerDebugStatePrinter.Print(session.Player);
        Console.WriteLine($"Total rendered events: {renderer.RenderCount}");
    }
}