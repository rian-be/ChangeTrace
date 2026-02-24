using System.CommandLine;
using ChangeTrace.Cli.Handlers;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Commands;

/// <summary>
/// Represents 'export' CLI command that exports a repository timeline to  JSON file.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliCommand"/> to define the command structure and associate a handler.</item>
/// <item>Registers itself as a singleton via <see cref="AutoRegisterAttribute"/>.</item>
/// <item>Defines arguments and options for specifying the repository, output file, GitHub token, and verbosity.</item>
/// <item>The actual execution logic is handled by <see cref="ExportCommandHandler"/>.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class ExportCommand : ICliCommand
{
    /// <summary>
    /// Gets the handler type responsible for executing this command.
    /// </summary>
    public Type HandlerType => typeof(ExportCommandHandler);
    public Type? Parent => null;
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing the 'export' CLI command.
    /// </summary>
    /// <returns>A configured <see cref="Command"/> with arguments and options.</returns>
    public Command Build()
    {
        var cmd = new Command("export", "Export repository timeline");
        
        var repoArg = new Argument<string>("repository")  {  Description = "Local path or HTTPS URL to Git repository" };
        var outputOpt = new Option<string>("--output", "-o")  { Description = "Output JSON file path",  DefaultValueFactory = _ => "timeline.gittrace" };
        var tokenOpt = new Option<string?>("--token", "-r")  { Description = "GitHub personal access token" };
        var verboseOpt = new Option<bool>("--verbose", "-v") { Description = "Enable verbose logging" };
        
        cmd.Arguments.Add(repoArg);
        cmd.Options.Add(outputOpt);
        cmd.Options.Add(tokenOpt);
        cmd.Options.Add(verboseOpt);
        
        return cmd;
    }
}