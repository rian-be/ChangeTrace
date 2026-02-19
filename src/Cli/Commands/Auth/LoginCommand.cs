using System.CommandLine;
using ChangeTrace.Cli.Handlers.Auth;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Auth;

/// <summary>
/// CLI command for logging in to a specified authentication provider.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to provide a <see cref="Command"/> instance for registration.</item>
/// <item>Exposes <see cref="HandlerType"/> pointing to <see cref="LoginCommandHandler"/> which performs the actual login.</item>
/// <item>Defines a single argument to specify the provider (e.g., GitHub, GitLab).</item>
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

    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'login' command.
    /// </summary>
    /// <returns>A fully configured <see cref="Command"/> with arguments.</returns>
    public Command Build()
    {
        var cmd = new Command("login", "Login to provider");

        var provider = new Argument<string>("provider") { Description = "Auth provider (github, gitlab, etc)" };
        cmd.Arguments.Add(provider);
        return cmd;
    }
}