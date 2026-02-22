using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Handler for 'workspace create' CLI command that creates a new workspace within an organization.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to create <see cref="WorkspaceProfile"/> objects.</item>
/// <item>Uses <see cref="IProfileStore{OrganizationProfile}"/> to validate the organization exists.</item>
/// <item>Uses <see cref="IProfileStore{WorkspaceProfile}"/> to save the new workspace.</item>
/// <item>Checks for existing workspace with the same name in the organization.</item>
/// <item>Displays a confirmation panel with workspace and organization details on success.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkCreateCommandHandler))]
internal sealed class WorkCreateCommandHandler(
    IProfileStore<OrganizationProfile> orgStore,
    IProfileStore<WorkspaceProfile> workspaceStore) : ICliHandler
{
    /// <summary>
    /// Executes 'workspace create' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var wsName = parseResult.GetValue<string>("name");
        var orgName = parseResult.GetValue<string>("--org");

        if (string.IsNullOrWhiteSpace(orgName) || string.IsNullOrWhiteSpace(wsName))
        {
            AnsiConsole.MarkupLine("[red]Organization name and workspace name are required[/]");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Creating workspace...", async ctx =>
            {
                ctx.Status("Validating organization...");

                var org = await orgStore.GetByNameAsync(orgName, ct);
                if (org == null)
                {
                    AnsiConsole.MarkupLine($"[red]Organization '{orgName}' not found[/]");
                    return;
                }

                ctx.Status("Checking existing workspaces...");
                var allWorkspaces = await workspaceStore.GetAllAsync(ct);
                var existing = allWorkspaces
                    .FirstOrDefault(w =>
                        w.OrganizationId == org.Id &&
                        w.Name.Equals(wsName, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    AnsiConsole.MarkupLine(
                        $"[yellow]Workspace '{wsName}' already exists in organization '{orgName}'[/]");
                    return;
                }

                ctx.Status("Saving new workspace...");
                var workspace = WorkspaceProfile.Create(org.Id, wsName);
                await workspaceStore.SaveAsync(workspace, ct);

                ctx.Status("Done!");
                DisplayConfirmation(workspace, org);
            });
    }

    /// <summary>
    /// Displays a confirmation panel for the created workspace.
    /// </summary>
    /// <param name="workspace">The newly created <see cref="WorkspaceProfile"/>.</param>
    /// <param name="organization">The parent <see cref="OrganizationProfile"/>.</param>
    private static void DisplayConfirmation(WorkspaceProfile workspace, OrganizationProfile organization)
    {
        var panel = new Panel(
                $"[bold]Workspace:[/] {workspace.Name}\n" +
                $"[bold]Organization:[/] {organization.Name}\n" +
                $"[bold]Created At:[/] {workspace.CreatedAt:u}\n" +
                $"[bold]Workspace ID:[/] {workspace.Id}")
            .Header("[green]Workspace Created[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}