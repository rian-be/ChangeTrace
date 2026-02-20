using System.CommandLine;

namespace ChangeTrace.Cli.Interfaces;

/// <summary>
/// Defines CLI command specification used to construct the command tree.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Responsible for building <see cref="Command"/> instance.</item>
/// <item>Optionally declares a parent command via <see cref="Parent"/> to support hierarchical composition.</item>
/// <item>Exposes <see cref="HandlerType"/> used to resolve and attach  command handler from DI container.</item>
/// <item>Consumed by the CLI composition pipeline to dynamically construct the full command tree.</item>
/// </list>
/// </remarks>
internal interface ICliCommand
{
    /// <summary>
    /// Gets the parent command definition type.
    /// </summary>
    /// <remarks>
    /// When <c>null</c>, the command is registered at the root level.
    /// </remarks>
    Type? Parent { get; }  
    
    /// <summary>
    /// Gets the <see cref="Type"/> of the handler responsible for executing this command.
    /// </summary>
    /// <remarks>
    /// When <c>null</c>, the command does not attach handler directly
    /// (for example, grouping or container commands).
    /// </remarks>
    Type? HandlerType { get; }
    
    /// <summary>
    /// Builds the <see cref="Command"/> instance representing this CLI command.
    /// </summary>
    /// <returns>
    /// A configured <see cref="Command"/> ready to be composed into the command tree.
    /// </returns>
    Command Build();
}