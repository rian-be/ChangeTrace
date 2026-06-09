using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Timelines;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Graphics.Window;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering.Factory;
using ChangeTrace.Cli.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers;

/// <summary>
/// Loads and plays a .gittrace timeline file.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ShowTimelineCommandHandler))]
internal sealed class ShowTimelineCommandHandler(
    ITimelineRepository repository,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory,
    IDiagnosticsProvider diagnostics) : ICliHandler
{
    /// <summary>
    /// Reads a timeline file and opens the player window.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;

        if (!File.Exists(filePath))
        {
            AnsiConsole.MarkupLine($"[red]File not found:[/] {Markup.Escape(filePath)}");
            return;
        }

        AnsiConsole.MarkupLine($"[green]Playing[/] {Markup.Escape(filePath)}");

        var timeline = await TimelineFileLoader.LoadAsync(repository, filePath, ct);
        var player = playerFactory.Create(timeline, initialSpeed: 1.5, acceleration: 2.5);

        using var debugWindow = new DebugWindow(diagnostics);
        using var window = new PlayerWindow(timeline, playerFactory, renderFactory, diagnostics);
        window.SetDebugWindow(debugWindow);
        window.Run();
    }
}
