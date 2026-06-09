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

namespace ChangeTrace.Cli.Handlers.Debug;

/// <summary>
/// Handles debug window playback for serialized timelines.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WindowDebugCommandHandler))]
internal sealed class WindowDebugCommandHandler(
    ITimelineRepository repository,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory,
    IDiagnosticsProvider diagnostics) : ICliHandler
{
    /// <summary>
    /// Loads timeline files and starts player windows with debug diagnostics.
    /// </summary>
    public async Task HandleAsync(
        ParseResult parseResult,
        CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;

        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[red]File not found: {filePath}[/]");
            return;
        }

        try
        {
            Console.WriteLine($"[cyan]Loading timeline from: {filePath}[/]");

            var fileSize = new FileInfo(filePath).Length;

            Console.WriteLine($"[cyan]File size: {fileSize} bytes[/]");

            var timeline = await TimelineFileLoader.LoadAsync(repository, filePath, ct);

            Console.WriteLine($"[green]Timeline loaded: {timeline.Events.Count} events[/]");

            using var debugWindow = new DebugWindow(diagnostics);
            using var window = new PlayerWindow(
                timeline,
                playerFactory,
                renderFactory,
                diagnostics);

            window.SetDebugWindow(debugWindow);
            window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to read or deserialize file: {ex.Message}[/]");
            Console.WriteLine($"[red]Exception type: {ex.GetType().Name}[/]");
            Console.WriteLine("[red]Stack trace:[/]");
            Console.WriteLine(ex.StackTrace);

            if (ex.InnerException is null)
                return;

            Console.WriteLine($"[red]Inner exception: {ex.InnerException.Message}[/]");
            Console.WriteLine("[red]Inner stack trace:[/]");
            Console.WriteLine(ex.InnerException.StackTrace);
        }
    }
}
