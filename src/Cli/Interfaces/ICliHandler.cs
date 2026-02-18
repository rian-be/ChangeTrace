using System.CommandLine;

namespace ChangeTrace.Cli.Interfaces;

/// <summary>
/// Represents handler for CLI commands.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implementations of this interface process parsed <see cref="ParseResult"/>.</item>
/// <item>Handles asynchronous execution of CLI commands with support for cancellation via <see cref="CancellationToken"/>.</item>
/// </list>
/// </remarks>
internal interface ICliHandler
{
    /// <summary>
    /// Executes the CLI command asynchronously based on the given parse result.
    /// </summary>
    /// <param name="parseResult">The result of parsing the CLI arguments.</param>
    /// <param name="ct">A token to observe for cancellation.</param>
    Task HandleAsync(ParseResult parseResult, CancellationToken ct);
}
