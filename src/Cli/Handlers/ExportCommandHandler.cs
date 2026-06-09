using System.CommandLine;
using System.Diagnostics;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.Cli.Prompts;
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
        var repoResult = ResolveRepository(parseResult.GetValue<string?>("repository"));
        if (repoResult is { IsSuccess: false })
        {
            AnsiConsole.MarkupLine($"[red]Failed:[/] {Markup.Escape(repoResult.Error!)}");
            return;
        }

        var repo = repoResult.Value!;
        var explicitOutput = parseResult.GetValue<string?>("--output");
        var token = parseResult.GetValue<string?>("--token");
        var enrichChoice = parseResult.GetValue<string?>("--enrich");
        var mergeDetectionChoice = parseResult.GetValue<bool?>("--merge-detection");
        var verbose = parseResult.GetValue<bool>("--verbose");
        var useGitCli = parseResult.GetValue<bool>("--git-cli");
        var noRenames = parseResult.GetValue<bool>("--no-renames");
        var exportedAt = DateTimeOffset.UtcNow;

        var output = explicitOutput;
        var workspace = workspaceContext.Current;
        var provider = TryDetectProvider(repo);

        if (string.IsNullOrWhiteSpace(output))
        {
            if (workspace == null)
            {
                var repoName = GetRepositoryName(repo);
                output = Path.Combine(Directory.GetCurrentDirectory(), $"{repoName}.gittrace");
            }
            else
            {
                output = await workspaceTimelineStorage.CreateTimelinePathAsync(
                    workspace,
                    repo,
                    exportedAt,
                    Ulid.NewUlid().ToString(),
                    ct);
            }
        }

        if (string.IsNullOrWhiteSpace(token))
        {
            if (provider is not null)
            {
                var session = await sessionAuthStore.GetSession(provider, ct);
                token = session?.AccessToken;
            }
        }

        var enrichmentKinds = ResolveEnrichmentKinds(enrichChoice);
        if (enrichmentKinds is null)
        {
            AnsiConsole.MarkupLine("[red]Invalid enrichment value.[/] Use [yellow]none[/] or [yellow]pull-requests[/].");
            return;
        }

        var includeMergeDetection = ResolveMergeDetection(mergeDetectionChoice);
        if (includeMergeDetection is null)
            return;

        var options = new ExportOptions
        {
            GitHubToken = provider == "github" ? token : null,
            GitLabToken = provider == "gitlab" ? token : null,
            IncludeMergeDetection = includeMergeDetection.Value,
            EnrichmentKinds = enrichmentKinds.Value,
            IncludeBranchEvents = true,
            HistoryBackend = useGitCli
                ? GitHistoryReaderBackend.GitCli
                : GitHistoryReaderBackend.LibGit2Sharp,
            DetectRenames = !noRenames,
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
        if (workspace == null)
        {
            var relativePath = Path.GetRelativePath(Directory.GetCurrentDirectory(), output);
            AnsiConsole.MarkupLine($"[blue]Tip:[/] Run [yellow]changetrace show {Markup.Escape(relativePath)}[/] to visualize the timeline.");
        }
    }

    private static ExportEnrichmentKind? ResolveEnrichmentKinds(string? enrichChoice)
    {
        if (!string.IsNullOrWhiteSpace(enrichChoice))
        {
            return enrichChoice.Trim().ToLowerInvariant() switch
            {
                "none" => ExportEnrichmentKind.None,
                "pull-requests" or "pr" or "prs" => ExportEnrichmentKind.PullRequests,
                _ => null
            };
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
            return ExportEnrichmentKind.PullRequests;

        return EnrichmentPrompt.SelectEnrichmentKinds();
    }

    private static bool? ResolveMergeDetection(bool? mergeDetectionChoice)
    {
        if (mergeDetectionChoice.HasValue)
            return mergeDetectionChoice.Value;

        if (!AnsiConsole.Profile.Capabilities.Interactive)
            return true;

        return MergeDetectionPrompt.SelectMergeDetection();
    }

    private static string? TryDetectProvider(string repository)
    {
        try
        {
            if (IsRemoteRepository(repository))
                return ProviderUrlHelper.DetectProvider(repository);

            var remoteOrigin = GetRemoteOriginUrl(repository);
            return remoteOrigin is null
                ? null
                : ProviderUrlHelper.DetectProvider(remoteOrigin);
        }
        catch
        {
            return null;
        }
    }

    private static string? GetRemoteOriginUrl(string repoPath)
    {
        try
        {
            using var process = new Process
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
            process.StartInfo.ArgumentList.Add(repoPath);
            process.StartInfo.ArgumentList.Add("remote");
            process.StartInfo.ArgumentList.Add("get-url");
            process.StartInfo.ArgumentList.Add("origin");

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            var remoteUrl = output.Trim();
            return string.IsNullOrWhiteSpace(remoteUrl) ? null : remoteUrl;
        }
        catch
        {
            return null;
        }
    }

    private static RepositoryResolution ResolveRepository(string? repository)
    {
        if (!string.IsNullOrWhiteSpace(repository) && IsRemoteRepository(repository))
            return RepositoryResolution.Success(repository);

        var startPath = string.IsNullOrWhiteSpace(repository)
            ? Directory.GetCurrentDirectory()
            : Path.GetFullPath(repository);

        if (File.Exists(startPath))
            startPath = Path.GetDirectoryName(startPath)!;

        if (!Directory.Exists(startPath))
            return RepositoryResolution.Failure($"Directory not found: {startPath}");

        var gitRoot = FindGitRoot(startPath);
        return gitRoot is null
            ? RepositoryResolution.Failure(
                $"No Git repository found from '{startPath}'. This command needs a real Git checkout with a .git directory or gitfile.")
            : RepositoryResolution.Success(gitRoot);
    }

    private static string? FindGitRoot(string startPath)
    {
        return FindGitRootWithGit(startPath) ?? FindGitRootByWalkingParents(startPath);
    }

    private static string? FindGitRootWithGit(string startPath)
    {
        try
        {
            using var process = new Process
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
            process.StartInfo.ArgumentList.Add(startPath);
            process.StartInfo.ArgumentList.Add("rev-parse");
            process.StartInfo.ArgumentList.Add("--show-toplevel");

            process.Start();
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
                return null;

            var root = output.Trim();
            return Directory.Exists(root) ? root : null;
        }
        catch
        {
            return null;
        }
    }

    private static string? FindGitRootByWalkingParents(string startPath)
    {
        var current = new DirectoryInfo(startPath);
        while (current is not null)
        {
            var dotGitPath = Path.Combine(current.FullName, ".git");
            if (Directory.Exists(dotGitPath) || File.Exists(dotGitPath))
                return current.FullName;

            current = current.Parent;
        }

        return null;
    }

    private static bool IsRemoteRepository(string repository)
    {
        if (Uri.TryCreate(repository, UriKind.Absolute, out var uri))
            return uri.Scheme is "http" or "https" or "ssh" or "git";

        var atIndex = repository.IndexOf('@');
        var colonIndex = repository.IndexOf(':');
        return atIndex > 0 && colonIndex > atIndex;
    }

    private static string GetRepositoryName(string repository)
    {
        if (IsRemoteRepository(repository))
        {
            try
            {
                var url = repository.Replace("git@github.com:", "https://github.com/");
                if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
                {
                    var segments = uri.AbsolutePath
                        .Trim('/')
                        .Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (segments.Length >= 2)
                    {
                        var name = segments[1];
                        if (name.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
                            name = name[..^4];
                        return name;
                    }
                }
            }
            catch
            {
                // Fallback
            }
        }

        var repoDir = repository.TrimEnd(
            Path.DirectorySeparatorChar,
            Path.AltDirectorySeparatorChar);

        var dirName = Path.GetFileName(repoDir);

        if (dirName.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            dirName = dirName[..^4];

        return string.IsNullOrWhiteSpace(dirName) ? "repository" : dirName;
    }

    private readonly record struct RepositoryResolution(bool IsSuccess, string? Value, string? Error)
    {
        public static RepositoryResolution Success(string value) => new(true, value, null);
        public static RepositoryResolution Failure(string error) => new(false, null, error);
    }
}
