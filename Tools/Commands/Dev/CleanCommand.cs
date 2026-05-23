using ChangeTrace.Tools.Infrastructure;
using ChangeTrace.Tools.Infrastructure.Repositories;
using Spectre.Console;

namespace ChangeTrace.Tools.Commands.Dev;

/// <summary>
/// Removes generated build and temporary files.
/// </summary>
public sealed class CleanCommand(
    IRepositoryRootFinder repositoryRootFinder)
    : ICommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string Name => "clean";

    /// <summary>
    /// Executes cleanup for build and temporary directories.
    /// </summary>
    public Task<int> ExecuteAsync(CancellationToken cancellationToken = default)
    {
        var repoRoot = repositoryRootFinder.Find();

        var targets = new[]
        {
            Path.Combine(repoRoot, "publish"),
            Path.Combine(repoRoot, "Assets", ".icon_tmp"),
            Path.Combine(repoRoot, "bin"),
            Path.Combine(repoRoot, "obj")
        };

        var recursiveTargets = Directory.GetDirectories(repoRoot, "bin", SearchOption.AllDirectories)
            .Concat(Directory.GetDirectories(repoRoot, "obj", SearchOption.AllDirectories))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        var deleted = 0;
        var skipped = 0;
        var failed = 0;

        AnsiConsole.Write(new Rule("[yellow]Cleaning ChangeTrace[/]").LeftJustified());

        foreach (var target in targets.Concat(recursiveTargets).Distinct(StringComparer.OrdinalIgnoreCase))
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (!Directory.Exists(target))
            {
                skipped++;
                continue;
            }

            try
            {
                Directory.Delete(target, recursive: true);
                deleted++;

                AnsiConsole.MarkupLine($"[green]Deleted:[/] {Path.GetRelativePath(repoRoot, target)}");
            }
            catch (Exception ex)
            {
                failed++;

                AnsiConsole.MarkupLine(
                    $"[red]Failed:[/] {Path.GetRelativePath(repoRoot, target)} - {ex.Message}");
            }
        }

        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"Deleted: [green]{deleted}[/]");
        AnsiConsole.MarkupLine($"Skipped: [grey]{skipped}[/]");
        AnsiConsole.MarkupLine($"Failed:  {(failed == 0 ? "[green]0[/]" : $"[red]{failed}[/]")}");

        return Task.FromResult(failed == 0 ? 0 : 1);
    }
}