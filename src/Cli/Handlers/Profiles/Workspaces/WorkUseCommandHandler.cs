using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Sets the active workspace.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkUseCommandHandler))]
internal sealed class WorkUseCommandHandler(
    IProfileStore<OrganizationProfile> orgStore,
    IProfileStore<WorkspaceProfile> workspaceStore,
    IWorkspaceContext context) : ICliHandler
{
    /// <summary>
    /// Selects and activates a workspace.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var orgName = parseResult.GetValue<string?>("org");
        var wsName = parseResult.GetValue<string?>("name");

        if (string.IsNullOrWhiteSpace(orgName))
        {
            var organizations = (await orgStore.GetAllAsync(ct))
                .OrderBy(org => org.Name)
                .ToList();

            if (organizations.Count == 0)
            {
                AnsiConsole.MarkupLine("[yellow]No organizations found.[/]");
                return;
            }

            if (!AnsiConsole.Profile.Capabilities.Interactive)
            {
                AnsiConsole.MarkupLine("[red]Organization name is required in non-interactive terminals.[/]");
                return;
            }

            orgName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select organization")
                    .PageSize(8)
                    .AddChoices(organizations.Select(org => org.Name)));
        }

        if (string.IsNullOrWhiteSpace(orgName))
        {
            AnsiConsole.MarkupLine("[red]Organization name is required[/]");
            return;
        }

        var org = await orgStore.GetByNameAsync(orgName, ct);
        if (org == null)
        {
            AnsiConsole.MarkupLine($"[red]Organization '{orgName}' not found[/]");
            return;
        }

        var allWorkspaces = (await workspaceStore.GetAllAsync(ct))
            .Where(workspace => workspace.OrganizationId == org.Id)
            .OrderBy(workspace => workspace.Name)
            .ToList();

        if (string.IsNullOrWhiteSpace(wsName))
        {
            if (allWorkspaces.Count == 0)
            {
                AnsiConsole.MarkupLine(
                    $"[yellow]No workspaces found in organization '{Markup.Escape(orgName)}'.[/]");
                return;
            }

            if (!AnsiConsole.Profile.Capabilities.Interactive)
            {
                AnsiConsole.MarkupLine("[red]Workspace name is required in non-interactive terminals.[/]");
                return;
            }

            wsName = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"Select workspace in {org.Name}")
                    .PageSize(8)
                    .AddChoices(allWorkspaces.Select(workspace => workspace.Name)));
        }

        var workspace = allWorkspaces.FirstOrDefault(workspace =>
            workspace.Name.Equals(wsName, StringComparison.OrdinalIgnoreCase));

        if (workspace == null)
        {
            AnsiConsole.MarkupLine(
                $"[red]Workspace '{wsName}' not found in organization '{orgName}'[/]");
            return;
        }

        await context.SetCurrentAsync(workspace, ct);
        DisplayConfirmation(workspace, org);
    }

    /// <summary>
    /// Displays the activated workspace.
    /// </summary>
    private static void DisplayConfirmation(
        WorkspaceProfile workspace,
        OrganizationProfile organization)
    {
        var panel = new Panel(
                $"[bold]Workspace:[/] {workspace.Name}\n" +
                $"[bold]Organization:[/] {organization.Name}\n" +
                $"[bold]Workspace ID:[/] {workspace.Id}")
            .Header("[green]Workspace Activated[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}