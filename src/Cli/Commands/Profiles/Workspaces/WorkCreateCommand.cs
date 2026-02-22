using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

/// <summary>
/// Represents <c>workspace create</c> CLI command for creating a new workspace.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="WorkCommand"/>.</item>
/// <item>Execution handled by <see cref="WorkCreateCommandHandler"/>.</item>
/// <item>Requires workspace name argument.</item>
/// <item>Optionally accepts organization name via <c>--org</c>.</item>
/// <item>Optionally accepts comma-separated list of repositories via <c>--repos</c>.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkCreateCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type? HandlerType => typeof(WorkCreateCommandHandler);

    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(WorkCommand);

    /// <summary>
    /// Builds <see cref="Command"/> representing <c>workspace create</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with arguments and options.</returns>
    public Command Build() =>
        new("create", "Create workspace")
        {
            new Argument<string>("name") { Description = "Workspace name" },
            new Option<string>("--org") { Description = "Organization name" },
            new Option<string>("--repos") { Description = "Comma separated list of repos" }
        };
}