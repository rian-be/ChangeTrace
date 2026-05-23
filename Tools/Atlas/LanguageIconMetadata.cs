namespace ChangeTrace.Tools.Atlas;

/// <summary>
/// Metadata entry describing a single icon inside the atlas.
/// </summary>
public sealed record LanguageIconMetadata(
    int Index,
    string Name,
    string FileName,
    string EnumName,
    int X,
    int Y,
    int Width,
    int Height,
    double U,
    double V,
    double Uw,
    double Vh);