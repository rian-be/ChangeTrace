using System.CommandLine;
using ChangeTrace.Cli.Handlers.Debug;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Debug;

/// <summary>
/// CLI command for debugging Render functionality of the Player module.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class RenderDebugCommand : ICliCommand
{
    public Type HandlerType => typeof(RenderDebugCommandHandler);
    public Type Parent => typeof(DebugCommand);
    
    public Command Build() =>
        new("render", "Debug and test Render functionality")
        {
            new Argument<string>("file") { Description = "Path to .gittrace file to display" }
        };
}