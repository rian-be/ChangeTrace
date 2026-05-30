using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.Core.Diagnostics;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.GIt.Interfaces;
using ChangeTrace.Graphics.Window;
using ChangeTrace.Player.Factory;
using ChangeTrace.Rendering.Factory;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Workspaces;

/// <summary>
/// Plays timelines stored in a workspace.
/// </summary>
[AutoRegister(ServiceLifetime.Transient, typeof(WorkPlayCommandHandler))]
internal sealed class WorkPlayCommandHandler(
    IWorkspaceContext workspaceContext,
    IWorkspaceTimelineStorage timelineStorage,
    IProfileStore<OrganizationProfile> orgStore,
    IProfileStore<WorkspaceProfile> workspaceStore,
    ITimelineSerializer serializer,
    ITimelinePlayerFactory playerFactory,
    IRenderSystemFactory renderFactory,
    IDiagnosticsProvider diagnostics) : ICliHandler
{
    /// <summary>
    /// Opens and plays a selected timeline.
    /// </summary>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var promptWorkspace = parseResult.GetValue<bool>("--workspace");
        var workspace = promptWorkspace
            ? await SelectWorkspaceAsync(ct)
            : workspaceContext.Current;

        if (workspace == null)
        {
            if (!AnsiConsole.Profile.Capabilities.Interactive)
            {
                AnsiConsole.MarkupLine("[red]No active workspace selected.[/]");
                return;
            }

            workspace = await SelectWorkspaceAsync(ct);
            if (workspace == null)
                return;
        }

        var repoFilter = parseResult.GetValue<string?>("--repo");
        var select = parseResult.GetValue<bool>("--select");
        var timelines = await timelineStorage.ListTimelinesAsync(workspace, ct);
        var selected = SelectTimeline(timelines, repoFilter, select);

        if (selected == null)
        {
            var suffix = string.IsNullOrWhiteSpace(repoFilter)
                ? string.Empty
                : $" matching '{Markup.Escape(repoFilter)}'";

            AnsiConsole.MarkupLine($"[yellow]No timelines found for the active workspace{suffix}.[/]");
            return;
        }

        await PlayTimelineAsync(selected, ct);
    }

    /// <summary>
    /// Prompts for workspace selection.
    /// </summary>
    private async Task<WorkspaceProfile?> SelectWorkspaceAsync(CancellationToken ct)
    {
        var organizations = (await orgStore.GetAllAsync(ct))
            .OrderBy(org => org.Name)
            .ToList();

        if (organizations.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]No organizations found.[/]");
            return null;
        }

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[red]Workspace selection requires an interactive terminal.[/]");
            return null;
        }

        var organization = AnsiConsole.Prompt(
            new SelectionPrompt<OrganizationProfile>()
                .Title("Select organization")
                .PageSize(8)
                .UseConverter(org => org.Name)
                .AddChoices(organizations));

        var workspaces = (await workspaceStore.GetAllAsync(ct))
            .Where(workspace => workspace.OrganizationId == organization.Id)
            .OrderBy(workspace => workspace.Name)
            .ToList();

        if (workspaces.Count == 0)
        {
            AnsiConsole.MarkupLine($"[yellow]No workspaces found in organization '{Markup.Escape(organization.Name)}'.[/]");
            return null;
        }

        var selected = AnsiConsole.Prompt(
            new SelectionPrompt<WorkspaceProfile>()
                .Title($"Select workspace in {organization.Name}")
                .PageSize(8)
                .UseConverter(workspace => workspace.Name)
                .AddChoices(workspaces));

        await workspaceContext.SetCurrentAsync(selected, ct);
        return selected;
    }

    /// <summary>
    /// Loads and plays a timeline.
    /// </summary>
    private async Task PlayTimelineAsync(WorkspaceTimelineFile selected, CancellationToken ct)
    {
        if (!File.Exists(selected.Path))
        {
            AnsiConsole.MarkupLine($"[red]Timeline file not found:[/] {Markup.Escape(selected.Path)}");
            return;
        }

        AnsiConsole.MarkupLine($"[green]Playing[/] {Markup.Escape(selected.Path)}");

        var data = await File.ReadAllBytesAsync(selected.Path, ct);
        var timeline = await serializer.DeserializeAsync(data, ct);
        var player = playerFactory.Create(timeline, initialSpeed: 1.5, acceleration: 2.5);

        using var debugWindow = new DebugWindow(diagnostics);
        using var window = new PlayerWindow(timeline, playerFactory, renderFactory, diagnostics);
        window.SetDebugWindow(debugWindow);
        window.Run();
    }

    /// <summary>
    /// Selects a timeline from available candidates.
    /// </summary>
    private static WorkspaceTimelineFile? SelectTimeline(
        IReadOnlyList<WorkspaceTimelineFile> timelines,
        string? repoFilter,
        bool prompt)
    {
        var candidates = string.IsNullOrWhiteSpace(repoFilter)
            ? timelines
            : timelines.Where(timeline => MatchesRepository(timeline, repoFilter)).ToList();

        var ordered = candidates
            .OrderByDescending(timeline => timeline.Metadata?.ExportedAtUtc ?? timeline.LastModifiedUtc)
            .ToList();

        if (ordered.Count == 0)
            return null;

        if (!prompt || ordered.Count == 1)
            return ordered[0];

        if (!AnsiConsole.Profile.Capabilities.Interactive)
        {
            AnsiConsole.MarkupLine("[yellow]Timeline selection requires an interactive terminal; opening the newest match.[/]");
            return ordered[0];
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<WorkspaceTimelineFile>()
                .Title("Select timeline")
                .PageSize(10)
                .UseConverter(FormatTimelineChoice)
                .AddChoices(ordered));
    }

    /// <summary>
    /// Formats a timeline entry for interactive selection.
    /// </summary>
    private static string FormatTimelineChoice(WorkspaceTimelineFile timeline)
    {
        var metadata = timeline.Metadata;
        var repository = metadata?.RepositoryOwner is null
            ? metadata?.RepositoryName ?? "unknown"
            : $"{metadata.RepositoryOwner}/{metadata.RepositoryName}";
        var exported = metadata?.ExportedAtUtc.ToString("u") ?? timeline.LastModifiedUtc.ToString("u");

        return $"{repository} | {exported} | {Path.GetFileName(timeline.Path)}";
    }

    /// <summary>
    /// Determines whether a timeline matches the repository filter.
    /// </summary>
    private static bool MatchesRepository(WorkspaceTimelineFile timeline, string repoFilter)
    {
        var normalizedFilter = Normalize(repoFilter);
        var metadata = timeline.Metadata;

        if (metadata != null)
        {
            if (!string.IsNullOrWhiteSpace(metadata.RepositoryName) &&
                Normalize(metadata.RepositoryName) == normalizedFilter)
                return true;

            if (!string.IsNullOrWhiteSpace(metadata.RepositoryOwner) &&
                !string.IsNullOrWhiteSpace(metadata.RepositoryName) &&
                Normalize($"{metadata.RepositoryOwner}/{metadata.RepositoryName}") == normalizedFilter)
                return true;
        }

        return Normalize(timeline.Path).Contains(normalizedFilter, StringComparison.Ordinal);
    }

    /// <summary>
    /// Normalizes repository identifiers for comparison.
    /// </summary>
    private static string Normalize(string value)
        => value.Trim().Replace('\\', '/').ToLowerInvariant();
}