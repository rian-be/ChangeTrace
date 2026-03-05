using System.CommandLine;
using ChangeTrace.Cli.Commands.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Debug;

[AutoRegister(ServiceLifetime.Transient, typeof(PLayerDebugCommandHandler))]
internal sealed class PLayerDebugCommandHandler(
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory): ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;
        var timeline = await TimelineLoader.LoadAsync(serializer, filePath, ct);
        if (timeline == null)
            return;

        var player = playerFactory.Create(timeline, initialSpeed: 0.5, acceleration: 1.5);
       // player.OnProgress += p => Console.WriteLine($"Progress: {p:P1}");
        player.OnLoopCompleted += l => Console.WriteLine($"Loop completed: {l}");
       
        player.OnEvent += e =>
        {
            if (e.FilePath != null && !string.IsNullOrEmpty(e.FilePath))
            {
                var dt = DateTimeOffset.FromUnixTimeSeconds(e.Timestamp.UnixSeconds).ToLocalTime();
                Console.WriteLine($"[{dt:yyyy-MM-dd HH:mm:ss}] File: {e.FilePath}, Commit: {e.CommitSha}");
            }
        };
        
        var playResult = player.Play();
        if (!playResult.IsSuccess)
        {
            Console.WriteLine($"Failed to play: {playResult.Error}");
        }
        
        while (player.State == PlayerState.Playing && !ct.IsCancellationRequested)
        {
            await Task.Delay(500, ct);
          //  PrintPlayerState(player);
        }

        player.Stop();
        PrintPlayerState(player);
    }
    
    private void PrintPlayerState(ITimelinePlayer player)
    {
        var diag = player.GetDiagnostics();
        Console.WriteLine($"State: {diag.State}, Mode: {diag.Mode}, Direction: {diag.Direction}");
        Console.WriteLine($"Position: {diag.PositionSeconds:F2}/{diag.DurationSeconds:F2} ({diag.Progress:P1})");
        Console.WriteLine($"Speed: {diag.CurrentSpeed:F2}/{diag.TargetSpeed:F2}");
        Console.WriteLine($"Events: {diag.EventsFired}/{diag.TotalEvents}, Loops: {diag.LoopCount}");
    }
}