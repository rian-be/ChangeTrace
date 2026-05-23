namespace ChangeTrace.Tools.Infrastructure.Processes;

/// <summary>
/// Runs external processes and captures execution results.
/// </summary>
public interface IProcessRunner
{
    /// <summary>
    /// Runs a process and returns the execution result.
    /// </summary>
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a process in a specific working directory and returns the execution result.
    /// </summary>
    Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a process and throws if execution fails.
    /// </summary>
    Task RunRequiredAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs a process in a specific working directory and throws if execution fails.
    /// </summary>
    Task RunRequiredAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Checks whether a command exists in the current environment.
    /// </summary>
    Task<bool> HasCommandAsync(
        string command,
        CancellationToken cancellationToken = default);
}