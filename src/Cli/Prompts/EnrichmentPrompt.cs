using ChangeTrace.GIt.Options;
using Spectre.Console;

namespace ChangeTrace.Cli.Prompts;

/// <summary>
/// Prompts for export enrichment scope selection.
/// </summary>
internal static class EnrichmentPrompt
{
    /// <summary>
    /// Selects the enrichment scope to apply during export.
    /// </summary>
    public static ExportEnrichmentKind SelectEnrichmentKinds()
    {
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select export enrichment")
                .PageSize(8)
                .AddChoices(
                    "none",
                    "pull-requests"));

        return selection switch
        {
            "pull-requests" => ExportEnrichmentKind.PullRequests,
            _ => ExportEnrichmentKind.None
        };
    }
}
