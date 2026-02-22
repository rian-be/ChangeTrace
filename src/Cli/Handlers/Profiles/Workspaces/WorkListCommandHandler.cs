using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Handler for 'workspace list' CLI command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to list workspaces for an organization.</item>
/// <item>Uses <see cref="IWorkspaceStore"/> to query workspaces by organization name.</item>
/// <item>Outputs a formatted table with ID, name, and creation date.</item>
/// <item>Displays a warning if no workspaces are found.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkListCommandHandler))]
internal sealed class WorkListCommandHandler(IWorkspaceStore store) : ICliHandler
{
    /// <summary>
    /// Executes 'workspace list' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var orgName = parseResult.GetValue<string>("--org");

        if (string.IsNullOrWhiteSpace(orgName))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --org is required.");
            return;
        }

        await AnsiConsole.Status()
            .Spinner(Spinner.Known.Dots)
            .SpinnerStyle(Style.Parse("green"))
            .StartAsync("Loading workspaces...", async ctx =>
            {
                var workspaces = await store.GetByNameOrganization(orgName, ct);
                var list = workspaces.ToList();

                if (!list.Any())
                {
                    AnsiConsole.MarkupLine("[yellow] No workspaces found for this organization.[/]");
                    return;
                }

                await Task.Delay(300, ct);

                DisplayWorkspacesPanel(list, orgName);
            });
    }

    
    private static void DisplayWorkspacesPanel(IReadOnlyList<WorkspaceProfile> workspaces, string organizationName)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .AddColumn("ID")
            .AddColumn("Name")
            .AddColumn("Created At");

        foreach (var ws in workspaces)
        {
            table.AddRow(ws.Id.ToString(), ws.Name, ws.CreatedAt.ToString("u"));
        }

        var panel = new Panel(table)
            .Header($"[green]Workspaces for '{organizationName}'[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}