using System.Text;
using ChangeTrace.Tools.Atlas;
using ChangeTrace.Tools.Infrastructure;
using ChangeTrace.Tools.Infrastructure.Processes;
using ChangeTrace.Tools.Infrastructure.Repositories;
using Spectre.Console;

namespace ChangeTrace.Tools.Commands.Assets;

/// <summary>
/// Builds language icon atlas textures and generated metadata.
/// </summary>
public sealed class BuildLanguageIconAtlasCommand(
    IRepositoryRootFinder repositoryRootFinder,
    IProcessRunner processRunner)
    : ICommand
{
    /// <summary>
    /// Command name.
    /// </summary>
    public string Name => "assets atlas";

    /// <summary>
    /// Executes an atlas generation pipeline.
    /// </summary>
    public async Task<int> ExecuteAsync(
        CancellationToken cancellationToken = default)
    {
        var repoRoot = repositoryRootFinder.Find();

        var options = LanguageIconAtlasOptions.FromRepositoryRoot(repoRoot);

        Directory.CreateDirectory(options.RawIconDirectory);
        Directory.CreateDirectory(options.TemporaryDirectory);
        Directory.CreateDirectory(Path.GetDirectoryName(options.OutputCSharpFile)!);

        var imageMagick = await ResolveImageMagickAsync(cancellationToken);
        if (imageMagick is null)
        {
            AnsiConsole.MarkupLine(
                "[red]Missing dependency:[/] ImageMagick ([yellow]magick[/] or [yellow]convert[/]).");

            return 1;
        }

        if (!await processRunner.HasCommandAsync("rsvg-convert", cancellationToken))
        {
            AnsiConsole.MarkupLine(
                "[red]Missing dependency:[/] rsvg-convert.");

            return 1;
        }

        var svgFiles = Directory.GetFiles(options.RawIconDirectory, "*.svg")
            .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (svgFiles.Length == 0)
        {
            AnsiConsole.MarkupLineInterpolated(
                $"[yellow]No SVG files found in:[/] {options.RawIconDirectory}");

            return 1;
        }

        var atlasWidth = options.Cell * options.Columns;
        var atlasRows = (svgFiles.Length + options.Columns - 1) / options.Columns;
        var atlasHeight = atlasRows * options.Cell;

        if (Directory.Exists(options.TemporaryDirectory))
        {
            Directory.Delete(options.TemporaryDirectory, recursive: true);
        }

        Directory.CreateDirectory(options.TemporaryDirectory);

        try
        {
            await CreateEmptyAtlasAsync(
                imageMagick,
                options.OutputPng,
                atlasWidth,
                atlasHeight,
                cancellationToken);

            AnsiConsole.MarkupLineInterpolated(
                $"Using ImageMagick command: [cyan]{imageMagick}[/]");

            AnsiConsole.MarkupLineInterpolated(
                $"Generating atlas from [green]{svgFiles.Length}[/] SVG files...");

            var metadata = new List<LanguageIconMetadata>();

            await AnsiConsole.Progress()
                .StartAsync(async ctx =>
                {
                    var task = ctx.AddTask(
                        "[green]Processing SVG files[/]",
                        maxValue: svgFiles.Length);

                    for (var index = 0; index < svgFiles.Length; index++)
                    {
                        cancellationToken.ThrowIfCancellationRequested();

                        var svgFile = svgFiles[index];

                        var icon = await ProcessIconAsync(
                            imageMagick,
                            svgFile,
                            index,
                            atlasWidth,
                            atlasHeight,
                            options,
                            cancellationToken);

                        metadata.Add(icon);

                        task.Description =
                            $"[blue]{icon.Name}[/] -> {icon.EnumName}";

                        task.Increment(1);
                    }
                });

            await WriteJsonMetadataAsync(
                metadata,
                atlasRows,
                atlasWidth,
                atlasHeight,
                options,
                cancellationToken);

            await WriteGeneratedEnumAsync(
                metadata,
                options,
                cancellationToken);

            AnsiConsole.MarkupLineInterpolated(
                $"[green]Saved:[/] {options.OutputPng}");

            AnsiConsole.MarkupLineInterpolated(
                $"[green]Saved:[/] {options.OutputJson}");

            AnsiConsole.MarkupLineInterpolated(
                $"[green]Saved:[/] {options.OutputCSharpFile}");

            return 0;
        }
        finally
        {
            if (Directory.Exists(options.TemporaryDirectory))
            {
                Directory.Delete(options.TemporaryDirectory, recursive: true);
            }
        }
    }

    /// <summary>
    /// Processes a single SVG icon and appends it into an atlas.
    /// </summary>
    private async Task<LanguageIconMetadata> ProcessIconAsync(
        string imageMagick,
        string svgFile,
        int index,
        int atlasWidth,
        int atlasHeight,
        LanguageIconAtlasOptions options,
        CancellationToken cancellationToken)
    {
        var fileName = Path.GetFileName(svgFile);
        var name = Path.GetFileNameWithoutExtension(svgFile);

        var enumName = LanguageIconNameNormalizer.ToPascalCase(name);

        var pngPath = Path.Combine(
            options.TemporaryDirectory,
            $"{name}.png");

        var alphaPath = Path.Combine(
            options.TemporaryDirectory,
            $"{name}_alpha.png");

        var maskPath = Path.Combine(
            options.TemporaryDirectory,
            $"{name}_mask.png");

        await processRunner.RunRequiredAsync(
            "rsvg-convert",
            [
                "-w",
                "128",
                "-h",
                "128",
                svgFile,
                "-o",
                pngPath
            ],
            cancellationToken);

        await processRunner.RunRequiredAsync(
            imageMagick,
            [
                pngPath,
                "-trim",
                "+repage",
                "-resize",
                $"{options.Cell - options.Padding * 2}x{options.Cell - options.Padding * 2}",
                "-gravity",
                "center",
                "-background",
                "none",
                "-extent",
                $"{options.Cell}x{options.Cell}",
                "-alpha",
                "extract",
                alphaPath
            ],
            cancellationToken);

        await processRunner.RunRequiredAsync(
            imageMagick,
            [
                "-size",
                $"{options.Cell}x{options.Cell}",
                "xc:white",
                alphaPath,
                "-alpha",
                "off",
                "-compose",
                "CopyOpacity",
                "-composite",
                maskPath
            ],
            cancellationToken);

        var column = index % options.Columns;
        var row = index / options.Columns;

        var x = column * options.Cell;
        var y = row * options.Cell;

        await processRunner.RunRequiredAsync(
            imageMagick,
            [
                options.OutputPng,
                maskPath,
                "-geometry",
                $"+{x}+{y}",
                "-composite",
                options.OutputPng
            ],
            cancellationToken);

        return new LanguageIconMetadata(
            Index: index,
            Name: name,
            FileName: fileName,
            EnumName: enumName,
            X: x,
            Y: y,
            Width: options.Cell,
            Height: options.Cell,
            U: (double)x / atlasWidth,
            V: (double)y / atlasHeight,
            Uw: (double)options.Cell / atlasWidth,
            Vh: (double)options.Cell / atlasHeight);
    }
    
    /// <summary>
    /// Creates empty transparent atlas texture.
    /// </summary>
    private async Task CreateEmptyAtlasAsync(
        string imageMagick,
        string outputPng,
        int width,
        int height,
        CancellationToken cancellationToken)
    {
        await processRunner.RunRequiredAsync(
            imageMagick,
            [
                "-size",
                $"{width}x{height}",
                "xc:none",
                outputPng
            ],
            cancellationToken);
    }

    /// <summary>
    /// Writes atlas metadata JSON file.
    /// </summary>
    private async Task WriteJsonMetadataAsync(
        IReadOnlyList<LanguageIconMetadata> icons,
        int rows,
        int atlasWidth,
        int atlasHeight,
        LanguageIconAtlasOptions options,
        CancellationToken cancellationToken)
    {
        var json = new StringBuilder();

        json.AppendLine("{");
        json.AppendLine($"  \"cell\": {options.Cell},");
        json.AppendLine($"  \"columns\": {options.Columns},");
        json.AppendLine($"  \"rows\": {rows},");
        json.AppendLine($"  \"width\": {atlasWidth},");
        json.AppendLine($"  \"height\": {atlasHeight},");
        json.AppendLine("  \"icons\": {");

        for (var i = 0; i < icons.Count; i++)
        {
            var icon = icons[i];

            var comma = i == icons.Count - 1
                ? string.Empty
                : ",";

            json.AppendLine($"    \"{icon.Name}\": {{");
            json.AppendLine($"      \"index\": {icon.Index},");
            json.AppendLine($"      \"enum\": \"{icon.EnumName}\",");
            json.AppendLine($"      \"file\": \"{icon.FileName}\",");
            json.AppendLine($"      \"x\": {icon.X},");
            json.AppendLine($"      \"y\": {icon.Y},");
            json.AppendLine($"      \"w\": {icon.Width},");
            json.AppendLine($"      \"h\": {icon.Height},");
            json.AppendLine($"      \"u\": {icon.U:0.########},");
            json.AppendLine($"      \"v\": {icon.V:0.########},");
            json.AppendLine($"      \"uw\": {icon.Uw:0.########},");
            json.AppendLine($"      \"vh\": {icon.Vh:0.########}");
            json.AppendLine($"    }}{comma}");
        }

        json.AppendLine("  }");
        json.AppendLine("}");

        await File.WriteAllTextAsync(
            options.OutputJson,
            json.ToString(),
            Encoding.UTF8,
            cancellationToken);
    }

    /// <summary>
    /// Writes generated language icon enum.
    /// </summary>
    private async Task WriteGeneratedEnumAsync(
        IReadOnlyList<LanguageIconMetadata> icons,
        LanguageIconAtlasOptions options,
        CancellationToken cancellationToken)
    {
        var cs = new StringBuilder();

        cs.AppendLine("// <auto-generated />");
        cs.AppendLine("// Generated by ChangeTrace.Tools");
        cs.AppendLine();
        cs.AppendLine("namespace ChangeTrace.Graphics.Rendering;");
        cs.AppendLine();
        cs.AppendLine("internal enum LanguageIcon");
        cs.AppendLine("{");

        foreach (var icon in icons)
        {
            cs.AppendLine($"    {icon.EnumName} = {icon.Index},");
        }

        cs.AppendLine("}");

        await File.WriteAllTextAsync(
            options.OutputCSharpFile,
            cs.ToString(),
            Encoding.UTF8,
            cancellationToken);
    }

    /// <summary>
    /// Resolves ImageMagick executable name.
    /// </summary>
    private async Task<string?> ResolveImageMagickAsync(
        CancellationToken cancellationToken)
    {
        if (await processRunner.HasCommandAsync(
                "magick",
                cancellationToken))
        {
            return "magick";
        }

        if (await processRunner.HasCommandAsync(
                "convert",
                cancellationToken))
        {
            return "convert";
        }

        return null;
    }
}