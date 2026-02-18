using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Extensions;

/// <summary>
/// Provides extension methods to bind CLI commands to their handlers via <see cref="IServiceProvider"/>.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Enables attaching an <see cref="ICliHandler"/> to a <see cref="Command"/> instance.</item>
/// <item>Resolves the handler from the provided <see cref="IServiceProvider"/> to support DI.</item>
/// <item>Executes the handler asynchronously with cancellation support.</item>
/// </list>
/// </remarks>
internal static class CommandBindingExtensions
{
    /// <summary>
    /// Attaches handler of the specified type to the <see cref="Command"/>.
    /// </summary>
    /// <param name="cmd">The command to attach the handler to.</param>
    /// <param name="services">The <see cref="IServiceProvider"/> used to resolve the handler instance.</param>
    /// <param name="handlerType">The <see cref="Type"/> of the handler implementing <see cref="ICliHandler"/>.</param>
    public static void AttachHandler(this Command cmd, IServiceProvider services, Type handlerType)
    {
        cmd.SetAction(async (parseResult, ct) =>
        {
            var handler = (ICliHandler)services.GetRequiredService(handlerType);
            await handler.HandleAsync(parseResult, ct);
        });
    }
}