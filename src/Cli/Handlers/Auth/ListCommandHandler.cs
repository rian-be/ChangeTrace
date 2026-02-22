using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Auth;

/// <summary>
/// Handler for 'list' CLI command that displays all authenticated sessions.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to retrieve and display authentication sessions.</item>
/// <item>Uses <see cref="IAuthService"/> to access stored <see cref="AuthSession"/> objects.</item>
/// <item>Outputs formatted list of providers and creation times to console.</item>
/// <item>Handles case where no providers are authenticated and prints message.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(ListCommandHandler))]
internal sealed class ListCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes 'list' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        IReadOnlyList<AuthSession> sessions;
        
        try
        {
            sessions = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync("Fetching authenticated sessions...", async ctx =>
                {
                    var result = await auth.ListProviders(ct);
                    ctx.Status($"Found {result.Count} session(s)");
                    await Task.Delay(300, ct); 
                    return result;
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red] Failed to list sessions: {ex.Message}[/]");
            return;
        }

        if (sessions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ No authenticated providers.[/]");
            return;
        }

        DisplaySessions(sessions);
    }
    
    /// <summary>
    /// Displays providers panel.
    /// </summary>
    /// <param name="sessions">Authenticated session to display.</param>
    private static void DisplaySessions(IReadOnlyList<AuthSession> sessions)
    {
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("Provider")
            .AddColumn("Created At");

        foreach (var s in sessions)
        {
            table.AddRow(s.Provider, s.CreatedAt.ToString("u"));
        }

        AnsiConsole.Write(
            new Panel(table)
                .Header("[green]Authenticated Providers[/]")
                .Padding(1, 1)
                .BorderColor(Color.Green)
        );
    }
}