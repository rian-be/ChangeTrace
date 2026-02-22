using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Organizations;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Organizations;

/// <summary>
/// Represents <c>org list</c> CLI command for listing organizations.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="OrgCommand"/>.</item>
/// <item>Execution handled by <see cref="OrgListCommandHandler"/>.</item>
/// <item>Optionally filter results by provider via <c>--provider</c> (<c>-p</c>).</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class OrgListCommand : ICliCommand
{
    /// <summary>
    /// Gets handler type for this command.
    /// </summary>
    public Type HandlerType => typeof(OrgListCommandHandler);

    /// <summary>
    /// Gets parent command.
    /// </summary>
    public Type Parent => typeof(OrgCommand);
    
    /// <summary>
    /// Builds <see cref="Command"/> representing <c>org list</c> command.
    /// </summary>
    /// <returns>Configured <see cref="Command"/> with optional filter options.</returns>
    public Command Build() =>
        new("list", "List all organizations")
        {
            new Option<string>("--provider", "-p") { Description = "Authentication provider (github, gitlab)" }
        };
}