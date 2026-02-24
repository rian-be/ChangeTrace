using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Results;
using ChangeTrace.GIt.Delegates;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers;

[AutoRegister(ServiceLifetime.Transient, typeof(ExportCommandHandler))]
internal sealed class ExportCommandHandler(IRepositoryExporter exporter): ICliHandler
{
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var repo = parseResult.GetValue<string>("repository")!;
        var output = parseResult.GetValue<string>("--output")!;
        var token = parseResult.GetValue<string?>("--token");
        var verbose = parseResult.GetValue<bool>("--verbose");
        
        var options = new ExportOptions
        {
            GitHubToken = token,
            IncludeMergeDetection = true,
            EnrichWithPullRequests = true,
            IncludeBranchEvents = true,
        };

        ProgressCallback? progress = verbose
            ? (stage, current, total, message) =>
            {
                AnsiConsole.MarkupLine($"[cyan]{stage}[/] ({current}/{total}) {message}");
            }
            : null;

        var result = await AnsiConsole.Status()
            .StartAsync<Result>("Exporting repository...", async ctx =>
            {
                ctx.Spinner(Spinner.Known.Dots);
                ctx.SpinnerStyle(Style.Parse("cyan"));

                return await exporter.ExportAndSaveAsync(repo, output, options, progress, ct);
            });


           AnsiConsole.MarkupLine(result.IsSuccess
               ? $"[green]Exported successfully to {output}[/]"
               : $"[red]Failed: {result.Error}[/]");
    }
}