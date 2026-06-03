using System.CommandLine;
using ChangeTrace.Cli.Commands.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Interfaces;
using ChangeTrace.Core.Timelines;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Debug;

[AutoRegister(ServiceLifetime.Transient, typeof(RenderDebugCommandHandler))]
internal sealed class RenderDebugCommandHandler(
    ISerializer<Timeline> serializer,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory,
    Core.Diagnostics.IDiagnosticsProvider diagnostics): ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;
        var timeline = await TimelineLoader.LoadAsync(serializer, filePath, ct);
        if (timeline == null)
            return;
        
        await CompositionRoot.RunAsync(timeline, playerFactory, renderFactory, diagnostics);
    }
}
