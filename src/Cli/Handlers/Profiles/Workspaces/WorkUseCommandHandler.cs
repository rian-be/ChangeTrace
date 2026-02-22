using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Handler for 'workspace use' CLI command that sets a workspace as the current active workspace.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to update the current <see cref="WorkspaceProfile"/> in <see cref="IWorkspaceContext"/>.</item>
/// <item>Validates that the organization exists using <see cref="IProfileStore{OrganizationProfile}"/>.</item>
/// <item>Validates that the workspace exists in the given organization using <see cref="IProfileStore{WorkspaceProfile}"/>.</item>
/// <item>Displays a confirmation panel with workspace and organization details on success.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkUseCommandHandler))]
internal sealed class WorkUseCommandHandler(
    IProfileStore<OrganizationProfile> orgStore,
    IProfileStore<WorkspaceProfile> workspaceStore,
    IWorkspaceContext context) : ICliHandler
{
    /// <summary>
    /// Executes 'workspace use' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var orgName = parseResult.GetValue<string>("org");
        var wsName = parseResult.GetValue<string>("name");

        if (string.IsNullOrWhiteSpace(orgName) || string.IsNullOrWhiteSpace(wsName))
        {
            AnsiConsole.MarkupLine("[red]Organization name and workspace name are required[/]");
            return;
        }

        var org = await orgStore.GetByNameAsync(orgName, ct);
        if (org == null)
        {
            AnsiConsole.MarkupLine($"[red]Organization '{orgName}' not found[/]");
            return;
        }

        var allWorkspaces = await workspaceStore.GetAllAsync(ct);
        var workspace = allWorkspaces.FirstOrDefault(w =>
            w.OrganizationId == org.Id &&
            w.Name.Equals(wsName, StringComparison.OrdinalIgnoreCase));

        if (workspace == null)
        {
            AnsiConsole.MarkupLine($"[red]Workspace '{wsName}' not found in organization '{orgName}'[/]");
            return;
        }

        await context.SetCurrentAsync(workspace, ct);
        DisplayConfirmation(workspace, org);
    }

    /// <summary>
    /// Displays confirmation panel indicating the workspace is now active.
    /// </summary>
    /// <param name="workspace">The <see cref="WorkspaceProfile"/> set as active.</param>
    /// <param name="organization">The parent <see cref="OrganizationProfile"/>.</param>
    private static void DisplayConfirmation(WorkspaceProfile workspace, OrganizationProfile organization)
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