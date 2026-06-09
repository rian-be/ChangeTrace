using Spectre.Console;

namespace ChangeTrace.Cli.Prompts;

/// <summary>
/// Prompts for merge detection selection during export.
/// </summary>
internal static class MergeDetectionPrompt
{
    /// <summary>
    /// Selects whether merge detection should be enabled.
    /// </summary>
    public static bool SelectMergeDetection()
        => AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Enable merge detection?")
                .PageSize(6)
                .AddChoices("yes", "no")) == "yes";
}
