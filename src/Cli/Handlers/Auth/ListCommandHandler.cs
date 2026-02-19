using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.CredentialTrace.Interfaces;

namespace ChangeTrace.Cli.Handlers.Auth;

/// <summary>
/// Handler for the 'list' CLI command that displays all authenticated sessions.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to retrieve and display authentication sessions.</item>
/// <item>Uses <see cref="IAuthService"/> to access stored AuthSession objects.</item>
/// <item>Outputs a formatted list of providers and creation times to the console.</item>
/// <item>Handles the case where no providers are authenticated and prints an appropriate message.</item>
/// </list>
/// </remarks>
internal sealed class ListCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes the 'list' command asynchronously.
    /// </summary>
    /// <param name="parseResult">The parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var sessions = await auth.ListAsync(ct);

        if (sessions.Count == 0)
        {
            Console.WriteLine("No authenticated providers.");
            return;
        }

        Console.WriteLine("Authenticated providers:");
        Console.WriteLine();

        foreach (var s in sessions)
        {
            Console.WriteLine($"{s.Provider,-10}  {s.CreatedAt:u}");
        }
    }
}