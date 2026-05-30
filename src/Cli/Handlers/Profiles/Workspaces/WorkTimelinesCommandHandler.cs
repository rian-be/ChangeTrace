using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Lists timelines stored in the active workspace.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkTimelinesCommandHandler))]
internal sealed class WorkTimelinesCommandHandler(
    IWorkspaceContext workspaceContext,
    IWorkspaceTimelineStorage timelineStorage) : ICliHandler
{
    /// <summary>
    /// Displays timelines for the active workspace.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var workspace = workspaceContext.Current;
        if (workspace == null)
        {
            AnsiConsole.MarkupLine("[red]No active workspace selected.[/]");
            return;
        }

        var timelines = await timelineStorage.ListTimelinesAsync(workspace, ct);
        if (timelines.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No timelines found for the active workspace.[/]");
            return;
        }

        DisplayTimelines(workspace, timelines);
    }

    /// <summary>
    /// Renders timeline files for a workspace.
    /// </summary>
    private static void DisplayTimelines(
        WorkspaceProfile workspace,
        IReadOnlyList<WorkspaceTimelineFile> timelines)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Repository")
            .AddColumn("Exported")
            .AddColumn("Size")
            .AddColumn("Path");

        foreach (var timeline in timelines)
        {
            var metadata = timeline.Metadata;
            var repository = metadata?.RepositoryOwner is null
                ? metadata?.RepositoryName ?? "unknown"
                : $"{metadata.RepositoryOwner}/{metadata.RepositoryName}";

            var exportedAt = metadata?.ExportedAtUtc.ToString("u")
                ?? timeline.LastModifiedUtc.ToString("u");

            table.AddRow(
                Markup.Escape(repository),
                exportedAt,
                FormatSize(timeline.SizeBytes),
                Markup.Escape(timeline.Path));
        }

        var panel = new Panel(table)
            .Header($"[green]Timelines for '{Markup.Escape(workspace.Name)}'[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Formats byte count into a readable file size.
    /// </summary>
    private static string FormatSize(long bytes)
    {
        if (bytes < 1024)
            return $"{bytes} B";

        var kb = bytes / 1024d;
        if (kb < 1024)
            return $"{kb:0.0} KB";

        return $"{kb / 1024d:0.0} MB";
    }
}
