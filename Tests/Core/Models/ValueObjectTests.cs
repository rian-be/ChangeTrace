using ChangeTrace.Core.Models;
using Xunit;

namespace ChangeTrace.Tests.Core.Models;

/// <summary>Tests validation and normalization rules for core value objects.</summary>
public sealed class ValueObjectTests
{
    /// <summary>CommitSha.Create trims and lowercases valid SHA input.</summary>
    [Theory]
    [InlineData("0123456", "0123456")]
    [InlineData("ABCDEF0123456789", "abcdef0123456789")]
    [InlineData("  abcdef0  ", "abcdef0")]
    public void CommitSha_Create_NormalizesValidSha(string input, string expected)
    {
        var result = CommitSha.Create(input);

        Assert.True(result.IsSuccess);
        Assert.Equal(expected, result.Value.Value);
    }

    /// <summary>CommitSha.Create rejects empty, too-short, and non-hex input.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("123456")]
    [InlineData("not-a-sha")]
    public void CommitSha_Create_RejectsInvalidSha(string input)
    {
        var result = CommitSha.Create(input);

        Assert.True(result.IsFailure);
    }

    /// <summary>FilePath.Create normalizes directory separators to forward slashes.</summary>
    [Fact]
    public void FilePath_Create_NormalizesSeparators()
    {
        var result = FilePath.Create(@"src\Core\File.cs");

        Assert.True(result.IsSuccess);
        Assert.Equal("src/Core/File.cs", result.Value.Value);
    }

    /// <summary>FilePath.Create rejects paths that traverse outside the repository root.</summary>
    [Theory]
    [InlineData("../secret.txt")]
    [InlineData("src/../secret.txt")]
    public void FilePath_Create_RejectsTraversal(string path)
    {
        var result = FilePath.Create(path);

        Assert.True(result.IsFailure);
    }
}
