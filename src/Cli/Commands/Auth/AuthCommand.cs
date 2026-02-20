using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Auth;

/// <summary>
/// Represents  auth CLI command for managing authentication.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate subcommands.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Does not have a direct handler (<see cref="HandlerType"/> is <c>null</c>); functionality is delegated to subcommands.</item>
/// <item>Defines and registers the following subcommands:
/// <list type="bullet">
/// <item><see cref="LoginCommand"/> — login to a provider.</item>
/// <item><see cref="LogoutCommand"/> — logout from a provider.</item>
/// <item><see cref="ListCommand"/> — list all authenticated providers.</item>
/// </list>
/// </item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class AuthCommand : ICliCommand
{
    public string Name => "auth";
    
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
        => new(Name, "Auth management");
}