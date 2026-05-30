using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Lists workspaces for an organization or all organizations.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkListCommandHandler))]
internal sealed class WorkListCommandHandler(
    IWorkspaceStore store,
    IProfileStore<OrganizationProfile> orgStore) : ICliHandler
{
    /// <summary>
    /// Displays available workspaces.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var orgName = parseResult.GetValue<string?>("--org");

        if (string.IsNullOrWhiteSpace(orgName))
        {
            await DisplayAllWorkspacesAsync(ct);
            return;
        }

        var workspaces = await store.GetByNameOrganization(orgName, ct);
        var list = workspaces.ToList();

        if (!list.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No workspaces found for this organization.[/]");
            return;
        }

        DisplayWorkspacesPanel(list, orgName);
    }

    /// <summary>
    /// Displays workspaces from all organizations.
    /// </summary>
    private async Task DisplayAllWorkspacesAsync(CancellationToken ct)
    {
        var organizations = (await orgStore.GetAllAsync(ct))
            .ToDictionary(org => org.Id, org => org.Name);

        var workspaces = (await store.GetAllAsync(ct))
            .OrderBy(workspace =>
                organizations.GetValueOrDefault(
                    workspace.OrganizationId,
                    workspace.OrganizationId.ToString()))
            .ThenBy(workspace => workspace.Name)
            .ToList();

        if (workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No workspaces found.[/]");
            return;
        }

        DisplayAllWorkspacesPanel(workspaces, organizations);
    }

    /// <summary>
    /// Renders workspaces for a single organization.
    /// </summary>
    private static void DisplayWorkspacesPanel(
        IReadOnlyList<WorkspaceProfile> workspaces,
        string organizationName)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Name")
            .AddColumn("Created At");

        foreach (var ws in workspaces)
        {
            table.AddRow(
                ws.Id.ToString(),
                ws.Name,
                ws.CreatedAt.ToString("u"));
        }

        var panel = new Panel(table)
            .Header($"[green]Workspaces for '{organizationName}'[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }

    /// <summary>
    /// Renders workspaces grouped by organization.
    /// </summary>
    private static void DisplayAllWorkspacesPanel(
        IReadOnlyList<WorkspaceProfile> workspaces,
        IReadOnlyDictionary<Ulid, string> organizations)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("Organization")
            .AddColumn("Workspace")
            .AddColumn("Created At");

        foreach (var ws in workspaces)
        {
            table.AddRow(
                organizations.GetValueOrDefault(
                    ws.OrganizationId,
                    ws.OrganizationId.ToString()),
                ws.Name,
                ws.CreatedAt.ToString("u"));
        }

        var panel = new Panel(table)
            .Header("[green]Workspaces[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}