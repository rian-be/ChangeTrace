using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkPlayCommand : ICliCommand
{
    public Type HandlerType => typeof(WorkPlayCommandHandler);

    public Type Parent => typeof(WorkCommand);

    public Command Build()
        => new("play", "Play a timeline from the active workspace")
        {
            new Option<string?>("--repo", "-r")
            {
                Description = "Repository filter, for example microsoft/msquic or msquic. Defaults to the newest timeline."
            },
            new Option<bool>("--select", "-s")
            {
                Description = "Prompt for timeline selection instead of opening the newest match."
            },
            new Option<bool>("--workspace", "-w")
            {
                Description = "Prompt for workspace before playing."
            }
        };
}
