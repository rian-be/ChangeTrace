using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.GIt.Delegates;
using ChangeTrace.GIt.Helpers;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.GIt.Options;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers;

/// <summary>
/// CLI handler responsible for exporting a Git repository into a ChangeTrace timeline file.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Resolves authentication token from CLI option or stored session.</item>
/// <item>Detects repository provider automatically.</item>
/// <item>Invokes <see cref="IRepositoryExporter"/> to perform export pipeline.</item>
/// <item>Displays progress and result using Spectre.Console UI.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(ExportCommandHandler))]
internal sealed class ExportCommandHandler(
    IAuthService sessionAuthStore,
    IRepositoryExporter exporter) : ICliHandler
{
    /// <summary>
    /// Executes the export command.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Task representing asynchronous command execution.</returns>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var repo = parseResult.GetValue<string>("repository")!;
        var output = parseResult.GetValue<string>("--output")!;
        var token = parseResult.GetValue<string?>("--token");
        var verbose = parseResult.GetValue<bool>("--verbose");
        
        if (string.IsNullOrWhiteSpace(token))
        {
            var provider = ProviderUrlHelper.DetectProvider(repo);
            var session = await sessionAuthStore.GetSession(provider, ct);
            token = session?.AccessToken;
        }

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
            .StartAsync("Exporting repository...", async ctx =>
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