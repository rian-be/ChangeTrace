using System.CommandLine;
using ChangeTrace.Cli.Handlers.Auth;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Auth;

/// <summary>
/// Represents  list tokens CLI command that lists all authenticated providers.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate a handler.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Exposes <see cref="HandlerType"/> pointing to <see cref="ListCommandHandler"/> which performs the listing logic.</item>
/// <item>Does not perform any authentication or session management itself; all execution is handled by the handler.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class ListCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(ListCommandHandler);
    public Type Parent => typeof(AuthCommand);
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'list' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with no arguments or options.</returns>
    public Command Build() => new Command("list", "List authenticated providers");
}