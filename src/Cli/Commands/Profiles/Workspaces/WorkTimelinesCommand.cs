using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkTimelinesCommand : ICliCommand
{
    public Type HandlerType => typeof(WorkTimelinesCommandHandler);

    public Type Parent => typeof(WorkCommand);

    public Command Build()
    {
        var cmd = new Command("timelines", "List timeline files stored for the active workspace");
        cmd.Aliases.Add("timeline");
        cmd.Aliases.Add("tl");
        return cmd;
    }
}
