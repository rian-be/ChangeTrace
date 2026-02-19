using System.CommandLine;
using System.Text.Json;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.GIt.Options;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles;

internal sealed class CreateProfileCommandHandler : ICliHandler
{
    private const string ProfilesFile = "profiles.json";

    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        AnsiConsole.MarkupLine("[bold green]Create a new export profile[/]");

        // 1️⃣ Nazwa profilu
        var profileName = AnsiConsole.Ask<string>("Enter profile name:");

        // 2️⃣ Ustawienia krok po kroku
        var includeFiles = await AnsiConsole.ConfirmAsync("Include file-level changes?", true, ct);
        var includeBranches = await AnsiConsole.ConfirmAsync("Include branch events?", true, ct);
        var includeMerge = await AnsiConsole.ConfirmAsync("Detect merge commits?", true, ct);
        var includePR = await AnsiConsole.ConfirmAsync("Enrich with pull requests?", true, ct);
        var maxCommits = AnsiConsole.Ask<int>("Maximum commits (0 = no limit)?");

        // 3️⃣ GitHub token (opcjonalnie)
        var token = AnsiConsole.Ask<string?>("GitHub token (optional)?");

        var profile = new ExportOptions
        {
            IncludeFileChanges = includeFiles,
            IncludeBranchEvents = includeBranches,
            IncludeMergeDetection = includeMerge,
            EnrichWithPullRequests = includePR,
            MaxCommits = maxCommits,
            GitHubToken = token
        };

        // 4️⃣ Wczytaj istniejące profile
        Dictionary<string, ExportOptions> profiles = new();
        if (File.Exists(ProfilesFile))
        {
            var json = await File.ReadAllTextAsync(ProfilesFile, ct);
            profiles = JsonSerializer.Deserialize<Dictionary<string, ExportOptions>>(json)
                       ?? new Dictionary<string, ExportOptions>();
        }

        // 5️⃣ Dodaj nowy profil
        profiles[profileName] = profile;

        // 6️⃣ Zapisz do pliku
        var serialized = JsonSerializer.Serialize(profiles, new JsonSerializerOptions { WriteIndented = true });
        await File.WriteAllTextAsync(ProfilesFile, serialized, ct);

        AnsiConsole.MarkupLine($"[green]Profile '{profileName}' saved successfully![/]");
    }
}
