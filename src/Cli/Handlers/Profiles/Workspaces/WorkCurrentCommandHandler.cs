using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Displays information about the active workspace.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkCurrentCommandHandler))]
internal sealed class WorkCurrentCommandHandler(
    IWorkspaceContext workspaceContext,
    IWorkspaceTimelineStorage timelineStorage,
    IProfileStore<OrganizationProfile> orgStore) : ICliHandler
{
    /// <summary>
    /// Shows the currently selected workspace and timeline count.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var workspace = workspaceContext.Current;
        if (workspace == null)
        {
            AnsiConsole.MarkupLine("[yellow]No active workspace selected.[/]");
            return;
        }

        var organization = await orgStore.GetAsync(workspace.OrganizationId, ct);
        var timelines = await timelineStorage.ListTimelinesAsync(workspace, ct);

        var panel = new Panel(
                $"[bold]Organization:[/] {organization?.Name ?? workspace.OrganizationId.ToString()}\n" +
                $"[bold]Workspace:[/] {workspace.Name}\n" +
                $"[bold]Workspace ID:[/] {workspace.Id}\n" +
                $"[bold]Timelines:[/] {timelines.Count}")
            .Header("[green]Active Workspace[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}
