using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

/// <summary>
/// Represents <c>workspace list</c> CLI command for listing workspaces.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="WorkCommand"/>.</item>
/// <item>Execution handled by <see cref="WorkListCommandHandler"/>.</item>
/// <item>Optionally filters results by organization using <c>--org</c> option.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkListCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type? HandlerType => typeof(WorkListCommandHandler);
    
    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(WorkCommand);

    /// <summary>
    /// Builds <see cref="Command"/> representing <c>workspace list</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with optional filtering options.</returns>
    public Command Build() =>
        new("list", "List workspaces")
        {
            new Option<string>("--org") { Description = "Organization name" }
        };
}