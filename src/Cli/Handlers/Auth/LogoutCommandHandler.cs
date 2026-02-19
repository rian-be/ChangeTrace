using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.CredentialTrace.Interfaces;

namespace ChangeTrace.Cli.Handlers.Auth;

/// <summary>
/// Handler for the 'logout' CLI command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to log out from a specified authentication provider.</item>
/// <item>Uses <see cref="IAuthService"/> to remove the persisted AuthSession for the given provider.</item>
/// <item>Outputs a confirmation message to the console upon successful logout.</item>
/// <item>Does not handle login or session creation; only removes existing sessions.</item>
/// </list>
/// </remarks>
internal sealed class LogoutCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes the 'logout' command asynchronously.
    /// </summary>
    /// <param name="parseResult">The parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var provider = parseResult.GetValue<string>("provider")!;

        await auth.LogoutAsync(provider, ct);
        Console.WriteLine($"Logged out from {provider}");
    }
}