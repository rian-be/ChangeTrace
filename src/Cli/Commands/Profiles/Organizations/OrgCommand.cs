using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Organizations;

/// <summary>
/// Represents <c>org</c> CLI command for organization management.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Groups organization-related subcommands.</item>
/// <item>Does not define execution logic; execution is delegated to subcommands.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Included in CLI command tree by the composition pipeline.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OrgCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c> because this command is a container for subcommands.
    /// </remarks>
    public Type? HandlerType => null;

    /// <summary>
    /// Gets parent command.
    /// </summary>
    /// <remarks>
    /// Returns <c>null</c>; this command is registered at root level.
    /// </remarks>
    public Type? Parent => null;

    /// <summary>
    /// Builds <see cref="Command"/> representing <c>org</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> for organization subcommands.</returns>
    public Command Build()
        => new("org", "Organization management");
}