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
/// Exports Git repository into ChangeTrace timeline file.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(ExportCommandHandler))]
internal sealed class ExportCommandHandler(
    IAuthService sessionAuthStore,
    IWorkspaceContext workspaceContext,
    IWorkspaceTimelineStorage workspaceTimelineStorage,
    IRepositoryExporter exporter) : ICliHandler
{
    /// <summary>
    /// Runs the repository export command.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var repo = parseResult.GetValue<string>("repository")!;
        var explicitOutput = parseResult.GetValue<string?>("--output");
        var token = parseResult.GetValue<string?>("--token");
        var verbose = parseResult.GetValue<bool>("--verbose");
        var exportedAt = DateTimeOffset.UtcNow;

        var output = explicitOutput;
        var workspace = workspaceContext.Current;

        if (string.IsNullOrWhiteSpace(output))
        {
            if (workspace == null)
            {
                AnsiConsole.MarkupLine(
                    "[red]Failed:[/] no active workspace selected. Use [yellow]workspace use <org> <name>[/] or pass [yellow]--output/-o[/].");
                return;
            }

            output = await workspaceTimelineStorage.CreateTimelinePathAsync(
                workspace,
                repo,
                exportedAt,
                Ulid.NewUlid().ToString(),
                ct);
        }

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

        if (result.IsFailure)
        {
            AnsiConsole.MarkupLine($"[red]Failed:[/] {Markup.Escape(result.Error ?? "Unknown error")}");
            return;
        }

        if (string.IsNullOrWhiteSpace(explicitOutput) && workspace != null)
            await workspaceTimelineStorage.SaveMetadataAsync(output, workspace, repo, exportedAt, ct);

        AnsiConsole.MarkupLine($"[green]Exported successfully to[/] {Markup.Escape(output)}");
    }
}