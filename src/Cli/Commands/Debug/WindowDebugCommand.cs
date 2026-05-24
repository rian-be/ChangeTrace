using System.CommandLine;
using ChangeTrace.Cli.Handlers.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Debug;

[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WindowDebugCommand : ICliCommand
{
    public Type HandlerType => typeof(WindowDebugCommandHandler);
    public Type Parent => typeof(DebugCommand);
    
    public Command Build() =>
        new("window", "Debug and test Render opentk functionality")
        {
            new Argument<string>("file") { Description = "Path to .gittrace file to display" }
        };
}