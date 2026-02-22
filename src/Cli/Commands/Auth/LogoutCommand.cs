using System.CommandLine;
using ChangeTrace.Cli.Handlers.Auth;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Auth;

/// <summary>
/// Represents logout CLI command that removes stored credentials for a specified provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate a handler.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Exposes <see cref="HandlerType"/> pointing to <see cref="LogoutCommandHandler"/> which performs the logout logic.</item>
/// <item>Defines a single argument for specifying the provider to log out from.</item>
/// <item>Does not perform session removal directly; all execution is handled by the handler.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class LogoutCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(LogoutCommandHandler);

    public Type Parent => typeof(AuthCommand);
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'logout' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with a single argument for the provider.</returns>
    public Command Build()
    {
        var cmd = new Command("logout", "Remove stored credentials");
        var provider = new Argument<string>("provider")
        {
            Description = "Provider to logout from",
            Arity = ArgumentArity.ZeroOrOne
        };
        
        cmd.Arguments.Add(provider);
        return cmd;
    }
}