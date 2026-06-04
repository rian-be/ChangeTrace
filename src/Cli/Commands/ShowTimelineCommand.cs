using System.CommandLine;
using ChangeTrace.Cli.Handlers;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class ShowTimelineCommand : ICliCommand
{
    public Type HandlerType => typeof(ShowTimelineCommandHandler);

    public Type? Parent => null;

    public Command Build()
        => new("show", "Play a .gittrace timeline file")
        {
            new Argument<string>("file")
            {
                Description = "Path to .gittrace file to play"
            }
        };
}
