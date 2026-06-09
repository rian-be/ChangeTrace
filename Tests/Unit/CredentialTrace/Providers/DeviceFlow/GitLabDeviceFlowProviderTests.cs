using System.Net;
using System.Net.Http;
using ChangeTrace.CredentialTrace.Providers.DeviceFlow;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Providers.DeviceFlow;

public sealed class GitLabDeviceFlowProviderTests
{
    [Fact]
    public async Task ValidateTokenAsync_ReturnsTrueForSuccessStatus()
        => await WithGitLabEnvironmentAsync(async () =>
        {
            using var http = new HttpClient(new StubHandler(HttpStatusCode.OK));
            var provider = new GitLabDeviceFlowProvider(http);

            var result = await provider.ValidateTokenAsync("token");

            Assert.True(result);
        });

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalseForUnauthorizedStatus()
        => await WithGitLabEnvironmentAsync(async () =>
        {
            using var http = new HttpClient(new StubHandler(HttpStatusCode.Unauthorized));
            var provider = new GitLabDeviceFlowProvider(http);

            var result = await provider.ValidateTokenAsync("token");

            Assert.False(result);
        });

    private static async Task WithGitLabEnvironmentAsync(Func<Task> action)
    {
        const string clientIdName = "CHANGETRACE_GITLAB_CLIENT_ID";
        const string baseUrlName = "CHANGETRACE_GITLAB_BASE_URL";

        var oldClientId = Environment.GetEnvironmentVariable(clientIdName);
        var oldBaseUrl = Environment.GetEnvironmentVariable(baseUrlName);

        try
        {
            Environment.SetEnvironmentVariable(clientIdName, "test-client");
            Environment.SetEnvironmentVariable(baseUrlName, "https://gitlab.com");
            await action();
        }
        finally
        {
            Environment.SetEnvironmentVariable(clientIdName, oldClientId);
            Environment.SetEnvironmentVariable(baseUrlName, oldBaseUrl);
        }
    }

    private sealed class StubHandler(HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
            => Task.FromResult(new HttpResponseMessage(statusCode));
    }
}
