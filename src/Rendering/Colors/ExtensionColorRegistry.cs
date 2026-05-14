namespace ChangeTrace.Rendering.Colors;

/// <summary>
/// Static registry mapping file extensions to deterministic display colors.
/// </summary>
/// <remarks>
/// Used by rendering systems to provide consistent language and file-type coloring.
/// Colors are stored as packed RGB hexadecimal values.
/// </remarks>
internal static class ExtensionColorRegistry
{
    /// <summary>
    /// Gets extension-to-color mapping table.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, uint> Colors =
        new Dictionary<string, uint>(StringComparer.OrdinalIgnoreCase)
        {
            // C# / .NET
            [".cs"]     = 0x7B68EE,
            [".csproj"] = 0x512BD4,
            [".sln"]    = 0x512BD4,
            // TypeScript / JavaScript
            [".ts"]     = 0x3178C6,
            [".tsx"]    = 0x3178C6,
            [".js"]     = 0xF0DB4F,
            [".jsx"]    = 0xF0DB4F,
            // Python
            [".py"]     = 0x3776AB,
            // Go
            [".go"]     = 0x00ACD7,
            // Rust
            [".rs"]     = 0xDEA584,
            // Java / Kotlin / Scala
            [".java"]   = 0xED8B00,
            [".kt"]     = 0x7F52FF,
            [".scala"]  = 0xDC322F,
            // C / C++ / Headers
            [".c"]      = 0x555555,
            [".cpp"]    = 0x00599C,
            [".cc"]     = 0x00599C,
            [".h"]      = 0x6A9FB5,
            [".hpp"]    = 0x6A9FB5,
            [".hxx"]    = 0x6A9FB5,
            // Ruby
            [".rb"]     = 0xCC342D,
            // PHP
            [".php"]    = 0x777BB4,
            // Swift
            [".swift"]  = 0xFA7343,
            // Dart / Flutter
            [".dart"]   = 0x0175C2,
            // Lua
            [".lua"]    = 0x000080,
            // Zig
            [".zig"]    = 0xF7A41D,
            // Elixir / Erlang
            [".ex"]     = 0x6E4A7E,
            [".exs"]    = 0x6E4A7E,
            [".erl"]    = 0xA90533,
            // R
            [".r"]      = 0x276DC3,
            // Markup / Config
            [".md"]     = 0xFFFFFF,
            [".json"]   = 0xCBCB41,
            [".yaml"]   = 0xCB6F29,
            [".yml"]    = 0xCB6F29,
            [".toml"]   = 0x9C4221,
            [".xml"]    = 0x0060AC,
            [".proto"]  = 0x4285F4,
            // Web
            [".html"]   = 0xE34C26,
            [".css"]    = 0x264DE4,
            [".scss"]   = 0xCD6799,
            [".sass"]   = 0xCD6799,
            [".less"]   = 0x1D365D,
            [".vue"]    = 0x42B883,
            [".svelte"] = 0xFF3E00,
            // Shell / DevOps
            [".sh"]     = 0x89E051,
            [".bash"]   = 0x89E051,
            [".ps1"]    = 0x012456,
            [".tf"]     = 0x623CE4,
            [".dockerfile"] = 0x2496ED,
            // Database
            [".sql"]    = 0xE38C00,
            // Shaders
            [".glsl"]   = 0xA8E600,
            [".hlsl"]   = 0xA8E600,
            [".vert"]   = 0xA8E600,
            [".frag"]   = 0xA8E600,
            // Text / Other
            [".txt"]    = 0xBBBBBB,
            [".log"]    = 0x999999,
            [".env"]    = 0xECD53F,
        };
}