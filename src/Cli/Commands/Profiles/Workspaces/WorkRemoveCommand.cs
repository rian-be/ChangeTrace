using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

/// <summary>
/// Represents <c>workspace remove</c> CLI command for removing a workspace.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="WorkCommand"/>.</item>
/// <item>Execution handled by <see cref="WorkRemoveCommandHandler"/>.</item>
/// <item>Requires workspace name argument.</item>
/// <item>Optionally specifies the organization using <c>--org</c> option.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkRemoveCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type? HandlerType => typeof(WorkRemoveCommandHandler);

    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(WorkCommand);

    /// <summary>
    /// Builds <see cref="Command"/> representing <c>workspace remove</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with required arguments and optional filtering options.</returns>
    public Command Build() =>
        new("remove", "Remove workspace")
        {
            new Argument<string>("name") { Description = "Workspace name" },
            new Option<string>("--org") { Description = "Organization name" }
        };
}