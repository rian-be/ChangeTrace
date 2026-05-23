using System.Diagnostics;
using System.Text;
using Spectre.Console;

namespace ChangeTrace.Tools.Infrastructure.Processes;

/// <summary>
/// Runs external processes and captures their execution result.
/// </summary>
public sealed class ProcessRunner : IProcessRunner
{
    /// <summary>
    /// Runs a process and returns the execution result.
    /// </summary>
    public Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default)
    {
        return RunAsync(
            fileName,
            arguments,
            workingDirectory: null,
            cancellationToken);
    }

    /// <summary>
    /// Runs a process in a specific working directory and returns the execution result.
    /// </summary>
    public async Task<ProcessResult> RunAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken = default)
    {
        string commandLine = ProcessResult.BuildCommandLine(
            fileName,
            arguments);

        var startInfo = new ProcessStartInfo
        {
            FileName = fileName,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        if (!string.IsNullOrWhiteSpace(workingDirectory))
            startInfo.WorkingDirectory = workingDirectory;

        foreach (string argument in arguments)
            startInfo.ArgumentList.Add(argument);

        using var process = new Process();
        process.StartInfo = startInfo;
        process.EnableRaisingEvents = true;

        var stderr = new StringBuilder();

        process.OutputDataReceived += (_, _) => { };

        process.ErrorDataReceived += (_, e) =>
        {
            if (e.Data is not null)
                stderr.AppendLine(e.Data);
        };

        try
        {
            if (!process.Start())
            {
                return ProcessResult.FailedToStart(
                    commandLine,
                    "Process.Start returned false.");
            }

            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync(cancellationToken);

            return new ProcessResult(
                CommandLine: commandLine,
                ExitCode: process.ExitCode,
                StandardError: stderr.ToString());
        }
        catch (OperationCanceledException)
        {
            TryKill(process);

            return new ProcessResult(
                CommandLine: commandLine,
                ExitCode: -2,
                StandardError: "Process was cancelled.");
        }
        catch (Exception ex)
        {
            return ProcessResult.FailedToStart(
                commandLine,
                ex.Message);
        }
    }

    /// <summary>
    /// Runs a process and throws if it fails.
    /// </summary>
    public Task RunRequiredAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken = default)
    {
        return RunRequiredAsync(
            fileName,
            arguments,
            workingDirectory: null,
            cancellationToken);
    }

    /// <summary>
    /// Runs a process in a specific working directory and throws if it fails.
    /// </summary>
    public async Task RunRequiredAsync(
        string fileName,
        IReadOnlyList<string> arguments,
        string? workingDirectory,
        CancellationToken cancellationToken = default)
    {
        ProcessResult result = await RunAsync(
            fileName,
            arguments,
            workingDirectory,
            cancellationToken);

        if (result.Success)
            return;

        if (!string.IsNullOrWhiteSpace(result.StandardError))
            AnsiConsole.WriteLine(result.StandardError.Trim());

        throw new InvalidOperationException(result.GetFailureMessage());
    }

    /// <summary>
    /// Checks whether a command exists in the current environment.
    /// </summary>
    public async Task<bool> HasCommandAsync(
        string command,
        CancellationToken cancellationToken = default)
    {
        string probe = OperatingSystem.IsWindows()
            ? "where"
            : "which";

        ProcessResult result = await RunAsync(
            probe,
            [command],
            cancellationToken);

        return result.Success;
    }

    /// <summary>
    /// Attempts to terminate the process tree.
    /// </summary>
    private static void TryKill(Process process)
    {
        try
        {
            if (!process.HasExited)
                process.Kill(entireProcessTree: true);
        }
        catch
        {
            // effort cleanup only.
        }
    }
}