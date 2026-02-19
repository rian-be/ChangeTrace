using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace ChangeTrace.Cli.Handlers.Auth;

/// <summary>
/// Handler for the 'login' CLI command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to perform the login process for a specified provider.</item>
/// <item>Uses <see cref="IAuthService"/> to authenticate and persist the session.</item>
/// <item>Outputs status messages to the console, including provider name and session creation time.</item>
/// <item>Automatically registered as a singleton via <see cref="AutoRegisterAttribute"/> for dependency injection.</item>
/// <item>Does not perform argument parsing; relies on <see cref="ParseResult"/> provided by the CLI framework.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class LoginCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes the login command asynchronously.
    /// </summary>
    /// <param name="parseResult">The parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var provider = parseResult.GetValue<string>("provider")!;

        Console.WriteLine($"Logging into {provider}...");
        var session = await auth.LoginAsync(provider, ct);

        Console.WriteLine();
        Console.WriteLine($"Logged in successfully.");
        Console.WriteLine($"Provider : {session.Provider}");
        Console.WriteLine($"Created  : {session.CreatedAt}");
    }
}

