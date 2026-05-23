using System.CommandLine;
using ChangeTrace.Cli.Handlers.Debug.Player;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Debug;

/// <summary>
/// Defines the CLI command for player debug playback.
/// </summary>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class PlayerDebugCommand : ICliCommand
{
    /// <summary>
    /// Handler type used to execute the command.
    /// </summary>
    public Type HandlerType => typeof(PlayerDebugCommandHandler);

    /// <summary>
    /// Parent command in the CLI tree.
    /// </summary>
    public Type Parent => typeof(DebugCommand);
    
    /// <summary>
    /// Builds the player debug command definition.
    /// </summary>
    public Command Build() =>
        new("player", "Debug and test Player functionality")
        {
            new Argument<string>("file")
            {
                Description = "Path to .gittrace file to display"
            }
        };
}