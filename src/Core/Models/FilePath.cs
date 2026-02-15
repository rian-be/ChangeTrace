using ChangeTrace.Core.Results;

namespace ChangeTrace.Core.Models;

/// <summary>
/// Represents validated file path with basic security checks.
/// 
/// Ensures non-empty, normalized paths with a maximum length of 4096 characters.
/// Prevents directory traversal and provides convenience accessors for
/// file name, directory, and extension.
/// </summary>
internal sealed record FilePath : ValueObject
{
    public string Value { get; }
    public string FileName => Path.GetFileName(Value);
    public string? Directory => Path.GetDirectoryName(Value);
    public string Extension => Path.GetExtension(Value);

    private FilePath(string value) => Value = value;

    /// <summary>
    /// Attempts to create <see cref="FilePath"/> from a string.
    /// Returns failure if the path is empty, too long, or contains traversal sequences.
    /// </summary>
    /// <param name="value">Candidate path string</param>
    /// <returns>Result containing a validated <see cref="FilePath"/> or an error</returns>
    public static Result<FilePath> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return Result<FilePath>.Failure("Path cannot be empty");

        value = value.Trim().Replace('\\', '/');

        if (value.Length > 4096)
            return Result<FilePath>.Failure("Path too long");

        // Security prevent path traversal
        if (value.Contains("../") || value.Contains("..\\"))
            return Result<FilePath>.Failure("Path traversal not allowed");

        return Result<FilePath>.Success(new FilePath(value));
    }

    /// <summary>
    /// Checks whether this file path is inside the specified directory.
    /// </summary>
    /// <param name="directory">Directory path to check against</param>
    /// <returns>True if the file path starts with the directory path; otherwise false</returns>
    public bool IsInDirectory(string directory)  =>
        Value.StartsWith(directory.TrimEnd('/') + "/", StringComparison.OrdinalIgnoreCase);

    public override string ToString() => Value;
    public static implicit operator string(FilePath path) => path.Value;
}