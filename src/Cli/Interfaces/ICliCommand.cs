using System.CommandLine;

namespace ChangeTrace.Cli.Interfaces;

/// <summary>
/// Represents a CLI command definition.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Responsible for constructing a <see cref="Command"/> instance for the CLI parser.</item>
/// <item>Exposes the <see cref="HandlerType"/> that will handle execution of this command.</item>
/// </list>
/// </remarks>
internal interface ICliCommand
{
    /// <summary>
    /// Builds the <see cref="Command"/> instance for this CLI command.
    /// </summary>
    /// <returns>A fully configured <see cref="Command"/> ready for registration with parser.</returns>
    Command Build();

    /// <summary>
    /// Gets the <see cref="Type"/> of handler responsible for executing this command.
    /// </summary>
    Type HandlerType { get; }
}