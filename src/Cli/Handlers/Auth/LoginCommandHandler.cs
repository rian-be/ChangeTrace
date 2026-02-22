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
/// Handler for 'login' CLI command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to perform login to specified provider.</item>
/// <item>Uses <see cref="IAuthService"/> to fetch or create <see cref="AuthSession"/>.</item>
/// <item>Displays provider name and creation time using Spectre.Console panel.</item>
/// <item>Handles login errors and displays formatted error panel.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Singleton)]
internal sealed class LoginCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes 'login' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var provider = parseResult.GetValue<string>("provider")!;
        
        AuthSession session;
        try
        {
            session = await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync($"Logging into [bold]{provider}[/]...", async ctx =>
                {
                    var s = await auth.FetchSession(provider, ct);
                    ctx.Status("Finalizing login...");
                    await Task.Delay(300, ct); 
                    return s;
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.Write(
                new Panel($"[red] Failed to login to [bold]{provider}[/]: {ex.Message}[/]")
                    .Border(BoxBorder.Rounded)
                    .BorderStyle(Color.Red)
                    .Padding(1, 1)
            );
            return;
        }
        
        DisplayConfirmation(session);
    }

    /// <summary>
    /// Displays login confirmation panel.
    /// </summary>
    /// <param name="session">Authenticated session to display.</param>
    private static void DisplayConfirmation(AuthSession session)
    {
        var panelText = new Markup(
            $"[bold]Provider:[/] {session.Provider}\n" +
            $"[bold]Created At:[/] {session.CreatedAt:u}"
        );

        AnsiConsole.Write(
            new Panel(panelText)
                .Header("[green]✅ Logged in successfully[/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Color.Green)
                .Padding(1, 1)
        );
    }
}