using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Organizations;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Organizations;

/// <summary>
/// Represents <c>org create</c> CLI command for creating a new organization.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="OrgCommand"/>.</item>
/// <item>Execution handled by <see cref="OrgCreateCommandHandler"/>.</item>
/// <item>Requires an organization name argument.</item>
/// <item>Optionally accepts provider via <c>--provider</c> (<c>-p</c>).</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OrgCreateCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type HandlerType => typeof(OrgCreateCommandHandler);

    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(OrgCommand);
    
    /// <summary>
    /// Builds <see cref="Command"/> representing <c>org create</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with arguments and options.</returns>
    public Command Build() =>
        new("create", "Create new organization (provider context)")
        {
            new Argument<string>("name") { Description = "Organization name" },
            new Option<string>("--provider", "-p") { Description = "Authentication provider (github, gitlab)" }
        };
}