namespace ChangeTrace.Tools.Atlas;

/// <summary>
/// Configuration for language icon atlas generation.
/// </summary>
public sealed record LanguageIconAtlasOptions(
    string RawIconDirectory,
    string TemporaryDirectory,
    string OutputPng,
    string OutputJson,
    string OutputCSharpFile,
    int Cell,
    int Columns,
    int Padding)
{
    /// <summary>
    /// Creates atlas configuration using repository root paths.
    /// </summary>
    public static LanguageIconAtlasOptions FromRepositoryRoot(string root)
    {
        return new LanguageIconAtlasOptions(
            RawIconDirectory: Path.Combine(root, "Assets", "RawIcons"),
            TemporaryDirectory: Path.Combine(root, "Assets", ".icon_tmp"),
            OutputPng: Path.Combine(root, "Assets", "language_icons.png"),
            OutputJson: Path.Combine(root, "Assets", "language_icons.json"),
            OutputCSharpFile: Path.Combine(root, "src", "Graphics", "Rendering", "LanguageIcon.generated.cs"),
            Cell: 32,
            Columns: 8,
            Padding: 5);
    }
}