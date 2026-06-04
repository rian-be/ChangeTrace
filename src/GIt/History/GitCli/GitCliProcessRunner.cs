using System.Diagnostics;

namespace ChangeTrace.GIt.History.GitCli;

/// <summary>
/// Runs Git CLI commands and captures their output.
/// </summary>
internal static class GitCliProcessRunner
{
    /// <summary>
    /// Runs a Git command and captures process output.
    /// </summary>
    public static async Task<GitCliResult> RunAsync(
        string repositoryPath,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        using var process = Create(repositoryPath, arguments);

        process.Start();

        var outputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var errorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken);

        return new GitCliResult(
            process.ExitCode,
            await outputTask,
            await errorTask);
    }

    /// <summary>
    /// Creates a configured Git process.
    /// </summary>
    public static Process Create(
        string repositoryPath,
        IReadOnlyList<string> arguments)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "git",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.StartInfo.ArgumentList.Add("-C");
        process.StartInfo.ArgumentList.Add(repositoryPath);

        foreach (var argument in arguments)
            process.StartInfo.ArgumentList.Add(argument);

        return process;
    }
}

/// <summary>
/// Git CLI command result.
/// </summary>
internal readonly record struct GitCliResult(
    int ExitCode,
    string Output,
    string Error);
