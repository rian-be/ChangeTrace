using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Organizations;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Organizations;

/// <summary>
/// Represents <c>org remove</c> CLI command for removing an organization.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="OrgCommand"/>.</item>
/// <item>Execution handled by <see cref="OrgRemoveCommandHandler"/>.</item>
/// <item>Requires organization name argument.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OrgRemoveCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type HandlerType => typeof(OrgRemoveCommandHandler);

    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(OrgCommand);
    
    /// <summary>
    /// Builds <see cref="Command"/> representing <c>org remove</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with required argument.</returns>
    public Command Build() =>
        new("remove", "Remove an organization")
        {
            new Argument<string>("name") { Description = "Organization name" }
        };
}