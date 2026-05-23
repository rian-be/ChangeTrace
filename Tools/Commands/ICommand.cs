namespace ChangeTrace.Tools.Commands;

/// <summary>
/// Base contract for executable CLI commands.
/// </summary>
public interface ICommand
{
    /// <summary>
    /// Command identifier used from CLI.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Executes command logic.
    /// </summary>
    Task<int> ExecuteAsync(
        CancellationToken cancellationToken = default);
}