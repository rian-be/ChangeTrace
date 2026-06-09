using System.Net;
using System.Net.Http;
using ChangeTrace.CredentialTrace.Providers.AuthorizationCodePkce;
using Xunit;

namespace ChangeTrace.Tests.CredentialTrace.Providers.AuthorizationCodePkce;

public sealed class CodebergOAuthProviderTests
{
    [Fact]
    public async Task ValidateTokenAsync_ReturnsTrueWhenAnyPresetAcceptsToken()
    {
        var requests = new List<Uri>();
        using var http = new HttpClient(new SequenceHandler(
            requests,
            HttpStatusCode.OK));

        var provider = new CodebergOAuthProvider(http);

        var result = await provider.ValidateTokenAsync("token");

        Assert.True(result);
        Assert.Equal(
            [
                new Uri("https://codeberg.org/api/v1/user")
            ],
            requests);
    }

    [Fact]
    public async Task ValidateTokenAsync_ReturnsFalseWhenAllPresetsRejectToken()
    {
        using var http = new HttpClient(new SequenceHandler(
            [],
            HttpStatusCode.Unauthorized));

        var provider = new CodebergOAuthProvider(http);

        var result = await provider.ValidateTokenAsync("token");

        Assert.False(result);
    }

    private sealed class SequenceHandler(
        ICollection<Uri> requests,
        params HttpStatusCode[] statuses) : HttpMessageHandler
    {
        private readonly Queue<HttpStatusCode> _statuses = new(statuses);

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            requests.Add(request.RequestUri!);

            var status = _statuses.Count > 0
                ? _statuses.Dequeue()
                : HttpStatusCode.Unauthorized;

            return Task.FromResult(new HttpResponseMessage(status));
        }
    }
}
