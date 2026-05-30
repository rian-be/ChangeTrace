using System.CommandLine;
using ChangeTrace.Cli.Handlers.Profiles.Workspaces;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands.Profiles.Workspaces;

/// <summary>
/// Represents the <c>workspace use</c> CLI command for selecting a workspace to activate.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Child command of <see cref="WorkCommand"/>.</item>
/// <item>Delegates execution to <see cref="WorkUseCommandHandler"/>.</item>
/// <item>Accepts optional organization and workspace arguments.</item>
/// <item>Prompts for missing values in interactive terminals.</item>
/// <item>Registered automatically as singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class WorkUseCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(WorkUseCommandHandler);

    /// <summary>
    /// Gets the parent command definition.
    /// </summary>
    public Type Parent => typeof(WorkCommand);

    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the <c>workspace use</c> command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with required arguments and optional organization filter.</returns>
    public Command Build()
    {
        var cmd = new Command("use", "Select active workspace");
        cmd.Aliases.Add("switch");
        cmd.Aliases.Add("select");
        cmd.Arguments.Add(new Argument<string?>("org")
        {
            Description = "Organization name",
            Arity = ArgumentArity.ZeroOrOne
        });
        cmd.Arguments.Add(new Argument<string?>("name")
        {
            Description = "Workspace name",
            Arity = ArgumentArity.ZeroOrOne
        });

        return cmd;
    }
}
