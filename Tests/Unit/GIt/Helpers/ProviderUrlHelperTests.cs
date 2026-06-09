using ChangeTrace.GIt.Helpers;
using Xunit;

namespace ChangeTrace.Tests.GIt.Helpers;

/// <summary>Tests Git provider detection from repository URLs.</summary>
public sealed class ProviderUrlHelperTests
{
    /// <summary>DetectProvider resolves supported providers from HTTPS, HTTP, and SSH repository formats.</summary>
    [Theory]
    [InlineData("https://github.com/rian-be/ChangeTrace.git")]
    [InlineData("http://github.com/rian-be/ChangeTrace")]
    [InlineData("git@github.com:rian-be/ChangeTrace.git")]
    public void DetectProvider_ReturnsGithubForSupportedGitHubFormats(string repository)
    {
        var provider = ProviderUrlHelper.DetectProvider(repository);

        Assert.Equal("github", provider);
    }

    /// <summary>DetectProvider resolves GitLab from HTTPS and SSH repository formats.</summary>
    [Theory]
    [InlineData("https://gitlab.com/rian-be/ChangeTrace.git")]
    [InlineData("http://gitlab.com/rian-be/ChangeTrace")]
    [InlineData("git@gitlab.com:rian-be/ChangeTrace.git")]
    public void DetectProvider_ReturnsGitlabForSupportedGitLabFormats(string repository)
    {
        var provider = ProviderUrlHelper.DetectProvider(repository);

        Assert.Equal("gitlab", provider);
    }

    /// <summary>DetectProvider resolves Codeberg from HTTPS and SSH repository formats.</summary>
    [Theory]
    [InlineData("https://codeberg.org/rian-be/ChangeTrace.git")]
    [InlineData("http://codeberg.org/rian-be/ChangeTrace")]
    [InlineData("git@codeberg.org:rian-be/ChangeTrace.git")]
    public void DetectProvider_ReturnsCodebergForSupportedCodebergFormats(string repository)
    {
        var provider = ProviderUrlHelper.DetectProvider(repository);

        Assert.Equal("codeberg", provider);
    }

    /// <summary>DetectProvider rejects blank repository input.</summary>
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void DetectProvider_RejectsBlankRepository(string repository)
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            ProviderUrlHelper.DetectProvider(repository));

        Assert.Equal("repository", exception.ParamName);
    }

    /// <summary>DetectProvider reports unsupported hosts after successful host extraction.</summary>
    [Fact]
    public void DetectProvider_RejectsUnsupportedHost()
    {
        var exception = Assert.Throws<NotSupportedException>(() =>
            ProviderUrlHelper.DetectProvider("https://gitea.example/rian-be/ChangeTrace.git"));

        Assert.Contains("gitea.example", exception.Message);
    }

    /// <summary>DetectProvider reports an invalid repository string when no host can be extracted.</summary>
    [Fact]
    public void DetectProvider_RejectsInvalidRepositoryString()
    {
        var exception = Assert.Throws<InvalidOperationException>(() =>
            ProviderUrlHelper.DetectProvider("not-a-repository"));

        Assert.Equal("Unable to determine repository host.", exception.Message);
    }
}
