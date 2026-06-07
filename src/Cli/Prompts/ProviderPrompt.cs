using ChangeTrace.CredentialTrace.Interfaces;
using Spectre.Console;

namespace ChangeTrace.Cli.Prompts;

/// <summary>
/// Prompts for authentication provider selection.
/// </summary>
internal static class ProviderPrompt
{
    /// <summary>
    /// Selects provider from registered authentication providers.
    /// </summary>
    public static string? SelectProvider(IEnumerable<IAuthProvider> providers)
    {
        var names = providers
            .Where(p => p.IsConfigured)
            .Select(p => p.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        var choices = names
            .OrderBy(name => name)
            .ToArray();

        if (choices.Length == 0)
        {
            AnsiConsole.MarkupLine("[red]No authentication providers are registered.[/]");
            return null;
        }

        return AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select authentication provider")
                .PageSize(8)
                .AddChoices(choices));
    }
}
