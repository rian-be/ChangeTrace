using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Core;
using System.Text.Json;
using System.Text.Json.Serialization;
using ChangeTrace.GIt.Interfaces;

namespace ChangeTrace.Cli.Handlers;

/// <summary>
/// CLI handler to read a .gittrace MessagePack file and display its content as JSON.
/// TEMP
/// </summary>
internal sealed class ShowTimelineCommandHandler(
    ITimelineSerializer serializer) : ICliHandler
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

            var json = JsonSerializer.Serialize(
                TimelineJsonDto.FromTimeline(timeline), 
                new JsonSerializerOptions { WriteIndented = true });

            Console.WriteLine(json);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[red]Failed to read or deserialize file: {ex.Message}[/]");
        }
    }
}

/// <summary>
/// Helper DTO for JSON display (map Timeline to serializable structure)
/// </summary>
internal sealed record TimelineJsonDto
{
    public string? Name { get; init; }
    public string? Repository { get; init; }
    public List<TraceEventJsonDto> Events { get; init; } = [];

    public static TimelineJsonDto FromTimeline(Timeline timeline)
    {
        return new TimelineJsonDto
        {
            Name = timeline.Name,
            Repository = timeline.RepositoryId?.ToString(),
            Events = timeline.Events.Select(TraceEventJsonDto.FromTraceEvent).ToList()
        };
    }
}

internal sealed record TraceEventJsonDto
{
    public long timestamp { get; init; }
    public string actor { get; init; } = "";
    public string target { get; init; } = "";

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? metadata { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? commitSha { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? filePath { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? commitType { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? branchName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? branchType { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public int? pullRequestNumber { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? prType { get; init; }

    public static TraceEventJsonDto FromTraceEvent(Core.Events.TraceEvent evt)
    {
        return new TraceEventJsonDto
        {
            timestamp = evt.Timestamp.UnixSeconds,
            actor = evt.Actor.Value,
            target = evt.Target,
            metadata = evt.Metadata,
            commitSha = evt.CommitSha?.Value,
            filePath = evt.FilePath?.Value,
            commitType = evt.CommitType?.ToString(),
            branchName = evt.BranchName?.Value,
            branchType = evt.BranchType?.ToString(),
            pullRequestNumber = evt.PullRequestNumber?.Value,
            prType = evt.PrType?.ToString()
        };
    }
}
