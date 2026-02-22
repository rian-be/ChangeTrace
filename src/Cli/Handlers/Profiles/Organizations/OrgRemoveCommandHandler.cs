using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Organizations;

[AutoRegister(ServiceLifetime.Transient, typeof(OrgRemoveCommandHandler))]
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