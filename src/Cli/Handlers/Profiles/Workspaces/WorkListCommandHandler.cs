using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

internal sealed class  WorkListCommandHandler(IProfileStore<WorkspaceProfile> store) : ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var orgId = Ulid.Parse(parseResult.GetValue<string>("--org"));
  /*
        var workspaces = await store.GetByOrganizationAsync(orgId, ct);

        var workspaceProfiles = workspaces.ToList();
        if (!workspaceProfiles.Any())
        {
            AnsiConsole.MarkupLine("[yellow]No workspaces for this organization[/]");
            return;
        }

        var table = new Table()
            .AddColumn("ID")
            .AddColumn("Name")
            .AddColumn("Created At");

        foreach (var ws in workspaceProfiles)
        {
            table.AddRow(ws.Id.ToString(), ws.Name, ws.CreatedAt.ToString("u"));
        }

        AnsiConsole.Write(table);
        */
    }
}