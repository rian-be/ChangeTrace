using System.CommandLine;
using ChangeTrace.Cli.Handlers;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands;

/// <summary>
/// Represents 'show' CLI command that reads a .gittrace file and displays its content as JSON.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate a handler.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Defines argument for specifying the timeline file path.</item>
/// <item>The actual execution logic is handled by <see cref="ShowTimelineCommandHandler"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class ShowTimelineCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(ShowTimelineCommandHandler);

    public Type Parent => null;
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'show' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with arguments.</returns>
    public Command Build()
    {
        var cmd = new Command("show", "Show a .gittrace timeline file as JSON");
        var fileArg = new Argument<string>("file") { Description = "Path to .gittrace file to display" };

        cmd.Arguments.Add(fileArg);
        return cmd;
    }
}