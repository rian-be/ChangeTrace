using ChangeTrace.GIt.History.GitCli;
using Xunit;

namespace ChangeTrace.Tests.GIt.Services;

public sealed class GitCliLogParserTests
{
    [Fact]
    public void MapCommit_UsesFallbackAuthor_WhenGitMetadataHasEmptyAuthor()
    {
        var commit = GitCliLogParser.MapCommit(
            CreateFields(author: ""),
            fileChanges: [],
            branches: []);

        Assert.NotNull(commit);
        Assert.Equal("Unknown", commit.Author.Value);
    }

    [Fact]
    public void MapCommit_TruncatesAuthor_WhenGitMetadataExceedsDomainLimit()
    {
        var longAuthor = new string('a', 240);

        var commit = GitCliLogParser.MapCommit(
            CreateFields(author: longAuthor),
            fileChanges: [],
            branches: []);

        Assert.NotNull(commit);
        Assert.Equal(200, commit.Author.Value.Length);
        Assert.Equal(new string('a', 200), commit.Author.Value);
    }

    [Fact]
    public void MapCommit_PreservesAuthor_WhenGitMetadataIsExactlyAtDomainLimit()
    {
        var boundaryAuthor = new string('b', 200);

        var commit = GitCliLogParser.MapCommit(
            CreateFields(author: boundaryAuthor),
            fileChanges: [],
            branches: []);

        Assert.NotNull(commit);
        Assert.Equal(boundaryAuthor, commit.Author.Value);
    }

    private static string[] CreateFields(
        string? author = "Author",
        string parents = "1111111111111111111111111111111111111111",
        string message = "message")
        =>
        [
            "0123456789abcdef0123456789abcdef01234567",
            author ?? "Author",
            "1717545600",
            parents,
            message
        ];
}
