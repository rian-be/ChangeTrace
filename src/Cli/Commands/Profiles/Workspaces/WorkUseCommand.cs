using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

/// <summary>
/// Represents the <c>workspace use</c> CLI command for selecting a workspace to activate.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="WorkCommand"/>.</item>
/// <item>Delegates execution to <see cref="WorkUseCommandHandler"/>.</item>
/// <item>Requires workspace name argument.</item>
/// <item>Optionally specifies the organization using <c>--org</c> option.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkUseCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(WorkUseCommandHandler);

    /// <summary>
    /// Gets the parent command definition.
    /// </summary>
    public Type Parent => typeof(WorkCommand);

    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the <c>workspace use</c> command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with required arguments and optional organization filter.</returns>
    public Command Build() => new("use", "Select workspace to play")
    {
        new Argument<string>("name") { Description = "Workspace name" },
        new Option<string>("--org") { Description = "Organization name" }
    };
}