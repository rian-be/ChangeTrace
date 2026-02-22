using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

[AutoRegister(ServiceLifetime.Transient, typeof(WorkRemoveCommandHandler))]
internal sealed class  WorkRemoveCommandHandler : ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        
    }
}