using System.CommandLine;
using ChangeTrace.Cli.Commands.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering;
using ChangeTrace.Rendering.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Debug;

[AutoRegister(ServiceLifetime.Transient, typeof(RenderDebugCommandHandler))]
internal sealed class RenderDebugCommandHandler(
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory): ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;
        var timeline = await TimelineLoader.LoadAsync(serializer, filePath, ct);
        if (timeline == null)
            return;
        
        await CompositionRoot.RunAsync(timeline, playerFactory, renderFactory);
    }
}