using System.Diagnostics;
using ChangeTrace.Tools.Infrastructure;
using ChangeTrace.Tools.Infrastructure.Processes;
using ChangeTrace.Tools.Infrastructure.Repositories;
using Spectre.Console;

namespace ChangeTrace.Tools.Commands.Publish;

/// <summary>
/// Builds publish artifacts for selected runtimes.
/// </summary>
public sealed class PublishCommand(
    IRepositoryRootFinder repositoryRootFinder,
    IProcessRunner processRunner)
    : ICommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string Name =>
        "publish";

    /// <summary>
    /// Executes runtime publishing pipeline.
    /// </summary>
    public async Task<int> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        string repoRoot =
            repositoryRootFinder.Find();

        string projectFile =
            Path.Combine(
                repoRoot,
                "ChangeTrace.csproj");

        string outputDirectory =
            Path.Combine(
                repoRoot,
                "publish");

        if (!File.Exists(projectFile))
        {
            AnsiConsole.MarkupLine(
                $"[red]Missing project file:[/] {projectFile}");

            return 1;
        }

        bool clean =
            await AnsiConsole.ConfirmAsync(
                "Clean publish directory first?",
                defaultValue: false,
                cancellationToken: cancellationToken);

        bool selfContained =
            await AnsiConsole.ConfirmAsync(
                "Build self-contained artifacts?",
                defaultValue: false,
                cancellationToken: cancellationToken);

        bool singleFile =
            await AnsiConsole.ConfirmAsync(
                "Create single-file executables?",
                defaultValue: false,
                cancellationToken: cancellationToken);

        List<string> runtimes =
            AnsiConsole.Prompt(
                new MultiSelectionPrompt<string>()
                    .Title("Select runtime(s) to publish:")
                    .NotRequired()
                    .InstructionsText(
                        "[grey](Press [blue]<space>[/] to toggle, [green]<enter>[/] to accept)[/]")
                    .AddChoices(
                        "win-x64",
                        "linux-x64",
                        "linux-arm64",
                        "osx-x64",
                        "osx-arm64"));

        if (runtimes.Count == 0)
        {
            runtimes =
            [
                "win-x64",
                "linux-x64",
                "linux-arm64",
                "osx-x64",
                "osx-arm64"
            ];
        }

        if (clean &&
            Directory.Exists(outputDirectory))
        {
            Directory.Delete(
                outputDirectory,
                recursive: true);
        }

        Directory.CreateDirectory(
            outputDirectory);

        AnsiConsole.MarkupLine(
            "[blue]Restoring packages...[/]");

        await processRunner.RunRequiredAsync(
            "dotnet",
            [
                "restore",
                projectFile
            ],
            cancellationToken);

        AnsiConsole.MarkupLine(
            "[blue]Building project...[/]");

        Stopwatch buildTimer =
            Stopwatch.StartNew();

        await processRunner.RunRequiredAsync(
            "dotnet",
            [
                "build",
                projectFile,
                "-c",
                "Release",
                "--no-restore"
            ],
            cancellationToken);

        buildTimer.Stop();

        List<PublishResult> results =
            [];

        await AnsiConsole.Progress()
            .AutoRefresh(true)
            .AutoClear(false)
            .HideCompleted(false)
            .Columns(
                new TaskDescriptionColumn(),
                new ProgressBarColumn(),
                new PercentageColumn(),
                new SpinnerColumn())
            .StartAsync(async ctx =>
            {
                IEnumerable<Task> tasks =
                    runtimes.Select(runtime =>
                    {
                        ProgressTask task =
                            ctx.AddTask(
                                $"[green]{runtime}[/]",
                                maxValue: 100);

                        task.IsIndeterminate = true;

                        return Task.Run(async () =>
                        {
                            PublishResult result =
                                await PublishRuntimeAsync(
                                    projectFile,
                                    outputDirectory,
                                    runtime,
                                    selfContained,
                                    singleFile,
                                    cancellationToken);

                            lock (results)
                            {
                                results.Add(result);
                            }

                            task.IsIndeterminate = false;
                            task.Value = 100;
                            task.StopTask();
                        }, cancellationToken);
                    });

                await Task.WhenAll(tasks);
            });

        WriteSummary(
            projectFile,
            outputDirectory,
            buildTimer.Elapsed,
            results);

        return results.All(x => x.Success)
            ? 0
            : 1;
    }

    /// <summary>
    /// Publishes project for a single runtime.
    /// </summary>
    private async Task<PublishResult> PublishRuntimeAsync(
        string projectFile,
        string outputDirectory,
        string runtime,
        bool selfContained,
        bool singleFile,
        CancellationToken cancellationToken)
    {
        Stopwatch timer =
            Stopwatch.StartNew();

        string runtimeOutputDirectory =
            Path.Combine(
                outputDirectory,
                runtime);

        List<string> arguments =
        [
            "publish",
            projectFile,
            "-c",
            "Release",
            "-r",
            runtime,
            "--self-contained",
            selfContained.ToString().ToLowerInvariant(),
            "--no-restore",
            "-p:UseAppHost=true",
            "-o",
            runtimeOutputDirectory
        ];

        if (singleFile)
        {
            arguments.Add("-p:PublishSingleFile=true");
            arguments.Add("-p:IncludeNativeLibrariesForSelfExtract=true");
        }

        bool success = true;

        try
        {
            await processRunner.RunRequiredAsync(
                "dotnet",
                arguments,
                cancellationToken);
        }
        catch
        {
            success = false;
        }

        if (success &&
            !OperatingSystem.IsWindows() &&
            !runtime.StartsWith(
                "win-",
                StringComparison.OrdinalIgnoreCase))
        {
            string executablePath =
                Path.Combine(
                    runtimeOutputDirectory,
                    "ChangeTrace");

            if (File.Exists(executablePath))
            {
                try
                {
                    await processRunner.RunRequiredAsync(
                        "chmod",
                        [
                            "+x",
                            executablePath
                        ],
                        cancellationToken);
                }
                catch
                {
                    success = false;
                }
            }
        }

        timer.Stop();

        double sizeMb =
            success &&
            Directory.Exists(runtimeOutputDirectory)
                ? GetDirectorySizeMb(runtimeOutputDirectory)
                : 0;

        return new PublishResult(
            Runtime: runtime,
            Success: success,
            OutputDirectory: runtimeOutputDirectory,
            Elapsed: timer.Elapsed,
            SizeMb: sizeMb);
    }

    /// <summary>
    /// Writes publish summary table.
    /// </summary>
    private static void WriteSummary(
        string projectFile,
        string outputDirectory,
        TimeSpan buildTime,
        IReadOnlyCollection<PublishResult> results)
    {
        AnsiConsole.WriteLine();

        AnsiConsole.Write(
            new Rule("[green]Publish Summary[/]")
                .LeftJustified());

        AnsiConsole.WriteLine(
            $"Project: {projectFile}");

        AnsiConsole.WriteLine(
            $"Output:  {outputDirectory}");

        AnsiConsole.WriteLine(
            $"Build:   {buildTime.TotalSeconds:F1}s");

        AnsiConsole.WriteLine();

        Table table =
            new();

        table.AddColumn("Runtime");
        table.AddColumn("Status");
        table.AddColumn("Output");
        table.AddColumn("Time");
        table.AddColumn("Size");

        foreach (PublishResult result in results.OrderBy(x => x.Runtime))
        {
            table.AddRow(
                result.Runtime,
                result.Success
                    ? "[green]Success[/]"
                    : "[red]Failed[/]",
                result.OutputDirectory,
                $"{result.Elapsed.TotalSeconds:F1}s",
                result.Success
                    ? $"{result.SizeMb:F1} MB"
                    : "-");
        }

        AnsiConsole.Write(table);
    }

    /// <summary>
    /// Calculates total directory size in megabytes.
    /// </summary>
    private static double GetDirectorySizeMb(
        string directory)
    {
        long bytes =
            new DirectoryInfo(directory)
                .GetFiles(
                    "*",
                    SearchOption.AllDirectories)
                .Sum(x => x.Length);

        return bytes / 1024.0 / 1024.0;
    }

    /// <summary>
    /// Publish result summary.
    /// </summary>
    private sealed record PublishResult(
        string Runtime,
        bool Success,
        string OutputDirectory,
        TimeSpan Elapsed,
        double SizeMb);
}