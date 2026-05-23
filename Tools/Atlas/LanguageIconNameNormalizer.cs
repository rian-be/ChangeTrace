using System.Text.RegularExpressions;

namespace ChangeTrace.Tools.Atlas;

/// <summary>
/// Utility for converting icon names into valid enum identifiers.
/// </summary>
public static partial class LanguageIconNameNormalizer
{
    /// <summary>
    /// Converts string into PascalCase identifier.
    /// </summary>
    public static string ToPascalCase(string input)
    {
        var parts = NonAlphaNumericRegex()
            .Split(input)
            .Where(x => !string.IsNullOrWhiteSpace(x));

        var result = string.Concat(parts.Select(x =>
            char.ToUpperInvariant(x[0]) + x[1..].ToLowerInvariant()));

        if (string.IsNullOrWhiteSpace(result))
            result = "Unknown";

        if (char.IsDigit(result[0]))
            result = "Icon" + result;

        return result;
    }
    
    /// <summary>
    /// Matches non-alphanumeric characters.
    /// </summary>
    [GeneratedRegex("[^a-zA-Z0-9]+")]
    private static partial Regex NonAlphaNumericRegex();
}