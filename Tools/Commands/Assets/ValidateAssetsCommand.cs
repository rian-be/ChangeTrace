using ChangeTrace.Tools.Atlas;
using ChangeTrace.Tools.Infrastructure;
using ChangeTrace.Tools.Infrastructure.Repositories;
using Spectre.Console;

namespace ChangeTrace.Tools.Commands.Assets;

/// <summary>
/// Validates language icon atlas source assets and generated outputs.
/// </summary>
public sealed class ValidateAssetsCommand(
    IRepositoryRootFinder repositoryRootFinder)
    : ICommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string Name => "assets validate";

    /// <summary>
    /// Executes asset validation pipeline.
    /// </summary>
    public Task<int> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var repoRoot = repositoryRootFinder.Find();

        var options = LanguageIconAtlasOptions.FromRepositoryRoot(repoRoot);

        var errors = new List<string>();
        var warnings = new List<string>();

        ValidateDirectories(options, errors);
        ValidateSvgFiles(options, errors, warnings);
        ValidateGeneratedFiles(options, warnings);

        AnsiConsole.WriteLine();

        if (errors.Count == 0 && warnings.Count == 0)
        {
            AnsiConsole.MarkupLine(
                "[green]Assets validation passed.[/]");

            return Task.FromResult(0);
        }

        if (warnings.Count > 0)
        {
            AnsiConsole.Write(new Rule("[yellow]Warnings[/]").LeftJustified());

            foreach (var warning in warnings)
            {
                AnsiConsole.MarkupLine($"[yellow]-[/] {warning}");
            }

            AnsiConsole.WriteLine();
        }

        if (errors.Count > 0)
        {
            AnsiConsole.Write(new Rule("[red]Errors[/]").LeftJustified());

            foreach (var error in errors)
            {
                AnsiConsole.MarkupLine($"[red]-[/] {error}");
            }

            return Task.FromResult(1);
        }

        return Task.FromResult(0);
    }

    /// <summary>
    /// Validates required asset directories.
    /// </summary>
    private static void ValidateDirectories(
        LanguageIconAtlasOptions options,
        ICollection<string> errors)
    {
        if (!Directory.Exists(options.RawIconDirectory))
        {
            errors.Add(
                $"Missing raw icon directory: {options.RawIconDirectory}");
        }
    }
    
    /// <summary>
    /// Validates source SVG icon files.
    /// </summary>
    private static void ValidateSvgFiles(
        LanguageIconAtlasOptions options,
        ICollection<string> errors,
        ICollection<string> warnings)
    {
        if (!Directory.Exists(options.RawIconDirectory))
            return;

        var svgFiles = Directory.GetFiles(
            options.RawIconDirectory,
            "*.svg",
            SearchOption.TopDirectoryOnly);

        if (svgFiles.Length == 0)
        {
            errors.Add(
                $"No SVG files found in: {options.RawIconDirectory}");

            return;
        }

        var duplicateNames = svgFiles
            .Select(Path.GetFileNameWithoutExtension)
            .GroupBy(x => x, StringComparer.OrdinalIgnoreCase)
            .Where(x => x.Count() > 1)
            .ToArray();

        foreach (var duplicate in duplicateNames)
        {
            errors.Add(
                $"Duplicate icon name: {duplicate.Key}");
        }

        foreach (var svg in svgFiles.OrderBy(x => x))
        {
            var fileName = Path.GetFileName(svg);
            var name = Path.GetFileNameWithoutExtension(svg);

            if (string.IsNullOrWhiteSpace(name))
            {
                errors.Add(
                    $"Invalid SVG filename: {fileName}");

                continue;
            }

            if (name.Length > 64)
            {
                warnings.Add(
                    $"Very long icon name: {fileName}");
            }

            if (name.Any(char.IsWhiteSpace))
            {
                warnings.Add(
                    $"Icon filename contains spaces: {fileName}");
            }

            if (name.StartsWith('-'))
            {
                warnings.Add(
                    $"Icon filename starts with '-': {fileName}");
            }

            var enumName = LanguageIconNameNormalizer.ToPascalCase(name);

            if (enumName == "Unknown")
            {
                warnings.Add(
                    $"Could not generate enum name for: {fileName}");
            }

            var fileInfo = new FileInfo(svg);

            if (fileInfo.Length == 0)
            {
                errors.Add(
                    $"Empty SVG file: {fileName}");
            }

            if (fileInfo.Length > 1024 * 1024)
            {
                warnings.Add(
                    $"Very large SVG file (>1MB): {fileName}");
            }

            try
            {
                using var stream = File.OpenRead(svg);

                if (stream.Length < 16)
                {
                    warnings.Add(
                        $"Suspiciously small SVG file: {fileName}");
                }
            }
            catch (Exception ex)
            {
                errors.Add(
                    $"Failed to read SVG file '{fileName}': {ex.Message}");
            }
        }
    }
    
    /// <summary>
    /// Validates generated atlas output files.
    /// </summary>
    private static void ValidateGeneratedFiles(
        LanguageIconAtlasOptions options,
        ICollection<string> warnings)
    {
        if (!File.Exists(options.OutputPng))
        {
            warnings.Add(
                $"Missing generated atlas PNG: {options.OutputPng}");
        }

        if (!File.Exists(options.OutputJson))
        {
            warnings.Add(
                $"Missing generated atlas JSON: {options.OutputJson}");
        }

        if (!File.Exists(options.OutputCSharpFile))
        {
            warnings.Add(
                $"Missing generated generated C# enum: {options.OutputCSharpFile}");
        }
    }
}