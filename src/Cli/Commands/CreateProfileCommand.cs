using System.CommandLine;
using ChangeTrace.Cli.Handlers;
using ChangeTrace.Cli.Handlers.Profiles;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands;

/// <summary>
/// Represents 'create-profile' CLI command that interactively creates a new export profile.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate a handler.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Optionally accepts a profile name as an argument; otherwise prompts interactively.</item>
/// <item>The actual execution logic is handled by <see cref="CreateProfileCommandHandler"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class CreateProfileCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(CreateProfileCommandHandler);

    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'create-profile' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with arguments and options.</returns>
    public Command Build()
    {
        var cmd = new Command("create-profile", "Interactively create a new export profile");
        
        var nameArg = new Argument<string?>("name") { Description = "Optional name of the profile" };
        cmd.Arguments.Add(nameArg);

        var skipTokenOption = new Option<bool>("--skip-token", "-s") { Description = "Skip prompting for GitHub token" };
        
        cmd.Options.Add(skipTokenOption);

        return cmd;
    }
}