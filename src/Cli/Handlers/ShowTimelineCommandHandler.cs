using System.CommandLine;
using System.Text.Json;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.Core.Specifications.Queries;
using ChangeTrace.Core.Specifications.Queries.Commits;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Graphics.Window;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering.Factory;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers;

/// <summary>
/// CLI handler to read .gittrace MessagePack file and display its content as JSON.
/// TEMP
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ShowTimelineCommandHandler))]
internal sealed class ShowTimelineCommandHandler(
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory,
    IDiagnosticsProvider diagnostics): ICliHandler
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
            var data = File.ReadAllBytes(filePath);
            var timeline = await serializer.DeserializeAsync(data, ct);

            //timeline.Normalize();

            var player = playerFactory.Create(
                timeline,
                initialSpeed: 1.5,
                acceleration: 2.5);

            var random = new Random();
            var eventsSample = timeline.Events
               // .OrderBy(_ => random.Next())
               .Take(20)
               .Select(evt => new
               {
                   Timestamp = evt.Core.Timestamp.UnixSeconds,
                   Actor = evt.Core.Actor?.Value,
                   Branch = evt.Branch?.Name?.Value,
                   CommitSha = evt.Commit?.Sha.Value,
                   Target = evt.Target,
                   Metadata = evt.Metadata?.ToString()
               })
               .ToList();

            var debugObject = new
            {
                Repository = timeline.RepositoryId != null
                    ? new
                    {
                        timeline.RepositoryId.Owner,
                        timeline.RepositoryId.Name
                    }
                    : null,
                //timeline.IsNormalized,
                Events = eventsSample
            };

            Console.WriteLine(
                JsonSerializer.Serialize(
                    debugObject,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    }
                )
            );

      /*
            var actorResult = ActorName.Create("Bryan Chen");
            if (!actorResult.IsSuccess)
            {
                Console.WriteLine($"Nieprawidłowa nazwa aktora: {actorResult.Error}");
                return Task.CompletedTask;
            }
            var actorc = actorResult.Value;
            Console.WriteLine($"[red] actorc: {actorc.ToString()}[/]");
            var spec = CommitQueries.EnrichedMerges()
                .And(CommitQueries.ByAuthor(actorc));

            var filteredEvents = timeline.Events
                .Where(e => spec.IsSatisfiedBy(e))
                .ToList();

            var grouped = filteredEvents
                .GroupBy(e => e.CommitSha)
                .Select(g => new { CommitSha = g.Key, Count = g.Count() })
                .ToList();

            foreach (var g in grouped)
            {
                Console.WriteLine($"{g.CommitSha}: {g.Count} razy");
            }

            */
           // var spec2 = OwnershipQueries.DirectoryOwners("src/Engine");
            var spec2 = OwnershipQueries.DirectoryOwners("extensions/csharp/snippets/");
            var authorsInDir = timeline.Events
                .Where(e => spec2.IsSatisfiedBy(e))
                .Select(e => e.Core.Actor?.Value)
                .Where(a => a != null)
                .Distinct()
                .ToList();

            Console.WriteLine($"Autorzy zmian w katalogu extensions/csharp/snippets/: {string.Join(", ", authorsInDir)}");

            var parentSpec = CommitRelationshipQueries.Parent("0a2f0cbc5c7ebc4573ba93c7b4c007efb1110856");
            var childSpec  = CommitRelationshipQueries.Children("0a2f0cbc5c7ebc4573ba93c7b4c007efb1110856");

            var parentEvent = timeline.Events.FirstOrDefault(e => parentSpec.IsSatisfiedBy(e));
            var childEvents = timeline.Events.Where(e => childSpec.IsSatisfiedBy(e)).ToList();

            var hierarchy = new
            {
                Commit = parentEvent.Commit != null ? new
                {
                    Timestamp = parentEvent.Core.Timestamp.UnixSeconds,
                    Actor = parentEvent.Core.Actor?.Value,
                    CommitSha = parentEvent.Commit?.Sha.Value,
                    Target = parentEvent.Target,
                    Metadata = parentEvent.Metadata?.ToString()
                } : null,

                Files = childEvents.Select(e => new
                {
                    Timestamp = e.Core.Timestamp.UnixSeconds,
                    Actor = e.Core.Actor?.Value,
                    CommitSha = e.Commit?.Sha.Value,
                    Target = e.Target,
                    FilePath = e.Metadata?.FilePath,
                    Metadata = e.Metadata?.ToString()
                }).ToList()
            };

           // Console.WriteLine(JsonSerializer.Serialize(hierarchy, new JsonSerializerOptions{ WriteIndented = true }));

            using var debugWindow = new DebugWindow(diagnostics);
            using var window = new PlayerWindow(timeline, playerFactory, renderFactory, diagnostics);
            window.SetDebugWindow(debugWindow);
            window.Run();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to read or deserialize file: {ex.Message}[/]");
            return;
        }
    }
}
