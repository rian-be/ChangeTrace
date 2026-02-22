using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Organizations;

internal sealed class OrgRemoveCommandHandler : ICliHandler
{
    public Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var name = parseResult.GetValue<string>("name");
        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Organization name is required.");
            return Task.CompletedTask;
        }

        return null;
    }
}