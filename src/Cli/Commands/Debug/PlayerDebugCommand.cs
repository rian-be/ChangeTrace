using System.CommandLine;
using ChangeTrace.Cli.Handlers.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Debug;

/// <summary>
/// CLI command for debugging Player functionality.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PlayerDebugCommand : ICliCommand
{
    public Type HandlerType => typeof(PLayerDebugCommandHandler);
    public Type Parent => typeof(DebugCommand);
    
    public Command Build() =>
        new("player", "Debug and test Player functionality")
        {
            new Argument<string>("file") { Description = "Path to .gittrace file to display" }
        };
}