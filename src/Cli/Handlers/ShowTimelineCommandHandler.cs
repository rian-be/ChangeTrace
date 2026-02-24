using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Events;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Player.Enums;
using ChangeTrace.Player.Factory;
using ChangeTrace.Player.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers;

/// <summary>
/// CLI handler to read a .gittrace MessagePack file and display its content as JSON.
/// TEMP
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ShowTimelineCommandHandler))]
internal sealed class ShowTimelineCommandHandler(
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory): ICliHandler
{

    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var filePath = parseResult.GetValue<string>("file")!;
        if (!File.Exists(filePath))
        {
            Console.WriteLine($"[red]File not found: {filePath}[/]");
            return;
        }

        try
        {
            var data = await File.ReadAllBytesAsync(filePath, ct);
            var timeline = await serializer.DeserializeAsync(data, ct);
            
            var player = playerFactory.Create(
                timeline,
                PlaybackMode.Loop,
                initialSpeed: 1.5,
                acceleration: 2.5);
            
            ConfigurePlayer(player);

            player.OnEvent += OnTraceEvent;
            player.OnStateChanged += s => Console.WriteLine($"\n  ← STATE → {s}");
            ThrottledProgress(player);
            player.OnLoopCompleted += n =>
            {
                Console.WriteLine($"\n  ← LOOP #{n} completed");
                Console.WriteLine(player.GetDiagnostics());
            };

            player.Play();
            //player.SeekRelative(52552);
            while (player.State == PlayerState.Playing)
                await Task.Delay(10, ct);

            Console.WriteLine("\nPlayback finished.");
           // Console.WriteLine(player.GetDiagnostics());
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to read or deserialize file: {ex.Message}[/]");
        }
    }

    private static void OnTraceEvent(TraceEvent evt)
    {
        Console.WriteLine(evt.ToString());
        //Console.WriteLine($"evt  {evt.Timestamp.UnixSeconds}  {evt.Actor.Value} → {evt.Target}");
    }
    private static void ConfigurePlayer(ITimelinePlayer player)
    {
        player.TargetSpeed = 0.1;
        player.Acceleration = 0.1;
        player.ApplyPreset(SpeedPreset.Normal);
    }

    private static void ThrottledProgress(ITimelinePlayer player)
    {
        double lastProgress = -1;
        player.OnProgress += p =>
        {
            if (!(Math.Abs(p - lastProgress) >= 0.01)) return; // update every 1%
            Console.Write($"\r  progress {p:P1}   ");
            lastProgress = p;
        };
    }
}
