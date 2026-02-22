using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Auth;

/// <summary>
/// Handles the <c>logout</c> CLI command to remove an authentication session.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> for asynchronous command execution.</item>
/// <item>Uses <see cref="IAuthService"/> to manage session removal for a specified provider.</item>
/// <item>If no provider is specified, prompts the user to select one from active sessions.</item>
/// <item>Displays a confirmation message upon successful logout.</item>
/// <item>Does not create or modify login sessions beyond removal.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(LogoutCommandHandler))]
internal sealed class LogoutCommandHandler(IAuthService auth) : ICliHandler
{
    /// <summary>
    /// Executes the <c>logout</c> command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed command-line arguments.</param>
    /// <param name="ct">Cancellation token to cancel the operation.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var provider = parseResult.GetValue<string>("provider")!;
      
        if (string.IsNullOrWhiteSpace(provider))
        {
            provider = await PromptForProvider(ct);
        }
        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync($"Logging out from [bold]{provider}[/]...", async ctx =>
                {
                    await auth.LogoutSession(provider: provider, ct);
                    ctx.Status("Finalizing logout...");
                    await Task.Delay(300, ct); 
                });
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red] Failed to logout from [bold]{provider}[/]: {ex.Message}[/]");
            return;
        }
        DisplayConfirmation(provider);
    }

    /// <summary>
    /// Prompts the user to select a provider from the list of active sessions.
    /// </summary>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>The selected provider name.</returns>
    private async Task<string> PromptForProvider(CancellationToken ct)
    {
        var sessions = await auth.ListProviders(ct);
        if (sessions.Count == 0)
        {
            AnsiConsole.MarkupLine("[yellow]⚠ No active sessions to log out from.[/]");
            throw new OperationCanceledException("No active sessions available.");
        }
        
        var table = new Table()
            .Border(TableBorder.Rounded)
            .BorderColor(Color.Grey)
            .AddColumn("Index")
            .AddColumn("Provider")
            .AddColumn("Created At");

        for (var i = 0; i < sessions.Count; i++)
        {
            var s = sessions[i];
            table.AddRow(
                (i + 1).ToString(),
                s.Provider,
                s.CreatedAt.ToString("u")
            );
        }

        AnsiConsole.Write(table);

        var index = AnsiConsole.Prompt(
            new TextPrompt<int>("Enter the [green]Index[/] of the provider to log out from:")
                .Validate(i =>
                {
                    if (i >= 1 && i <= sessions.Count)
                        return ValidationResult.Success();
                    return ValidationResult.Error("[red]Invalid index, select a number from the table[/]");
                })
        );

        return sessions[index - 1].Provider;
    }

    /// <summary>
    /// Displays confirmation message that logout succeeded.
    /// </summary>
    /// <param name="provider">The provider that was logged out.</param>
    private static void DisplayConfirmation(string provider) =>
        AnsiConsole.Write(
            new Panel($"[green] Successfully logged out from [bold]{provider}[/][/]")
                .Border(BoxBorder.Rounded)
                .BorderStyle(Color.Green)
                .Padding(1, 1)
        );
}