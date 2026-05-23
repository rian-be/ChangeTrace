namespace ChangeTrace.Tools.Infrastructure.Processes;

/// <summary>
/// Result of an external process execution.
/// </summary>
public sealed record ProcessResult(
    string CommandLine,
    int ExitCode,
    string StandardError)
{
    /// <summary>
    /// Indicates whether the process completed successfully.
    /// </summary>
    public bool Success => ExitCode == 0;

    /// <summary>
    /// Indicates whether the process failed.
    /// </summary>
    public bool Failed => !Success;

    /// <summary>
    /// Creates a result representing process startup failure.
    /// </summary>
    public static ProcessResult FailedToStart(
        string commandLine,
        string error)
    {
        return new ProcessResult(
            CommandLine: commandLine,
            ExitCode: -1,
            StandardError: error);
    }

    /// <summary>
    /// Builds a human-readable failure message.
    /// </summary>
    public string GetFailureMessage()
    {
        return $"Command failed with exit code {ExitCode}: {CommandLine}";
    }

    /// <summary>
    /// Builds a shell-style command line string.
    /// </summary>
    public static string BuildCommandLine(
        string fileName,
        IReadOnlyList<string> arguments)
    {
        if (arguments.Count == 0)
            return fileName;

        return fileName + " " + string.Join(
            " ",
            arguments.Select(QuoteArgument));
    }

    /// <summary>
    /// Quotes command line argument when needed.
    /// </summary>
    private static string QuoteArgument(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "\"\"";

        return value.Any(char.IsWhiteSpace)
            ? $"\"{value.Replace("\"", "\\\"")}\""
            : value;
    }
}