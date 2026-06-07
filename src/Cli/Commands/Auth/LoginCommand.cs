using System.CommandLine;
using ChangeTrace.Cli.Handlers.Auth;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Auth;

/// <summary>
/// CLI command for logging in to a specified authentication provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to provide a <see cref="Command"/> instance for registration.</item>
/// <item>Exposes <see cref="HandlerType"/> pointing to <see cref="LoginCommandHandler"/> which performs the actual login.</item>
/// <item>Accepts an optional provider argument and falls back to a selector when omitted.</item>
/// <item>Can be automatically registered as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class LoginCommand : ICliCommand
{
    /// <summary>
    /// Gets the type of the handler responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(LoginCommandHandler);
    public Type Parent => typeof(AuthCommand);
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'login' command.
    /// </summary>
    /// <returns>A fully configured <see cref="Command"/>.</returns>
    public Command Build()
    {
        var cmd = new Command("login", "Login to provider");

        var provider = new Option<string?>("--provider", "-p")
        {
            Description = "Authentication provider (github, gitlab, etc)"
        };

        cmd.Options.Add(provider);
        return cmd;
    }
}
