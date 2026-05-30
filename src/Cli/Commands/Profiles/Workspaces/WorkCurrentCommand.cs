using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkCurrentCommand : ICliCommand
{
    public Type HandlerType => typeof(WorkCurrentCommandHandler);

    public Type Parent => typeof(WorkCommand);

    public Command Build()
    {
        var cmd = new Command("current", "Show active workspace");
        cmd.Aliases.Add("status");
        cmd.Aliases.Add("ctx");
        return cmd;
    }
}
