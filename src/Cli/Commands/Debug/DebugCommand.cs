using System.CommandLine;
using ChangeTrace.Cli.Commands.Auth;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Debug;

/// <summary>
/// Represents debug CLI command for developer/debugging purposes.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define  command structure and associate subcommands.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Does not have a direct handler (<see cref="HandlerType"/> is <c>null</c>); functionality is delegated to subcommands.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class DebugCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// Returns <c>null</c> since execution is delegated to subcommands.
    /// </summary>
    public Type? HandlerType => null;

    public Type? Parent => null;
        
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'auth' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with subcommands for login, logout, and list.</returns>
    public Command Build()
        => new("debdev", "App Debbugers");
}