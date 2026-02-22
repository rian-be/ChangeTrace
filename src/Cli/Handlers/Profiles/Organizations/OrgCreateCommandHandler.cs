using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration.Discovery;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Organizations;

/// <summary>
/// Handler for 'organization create' CLI command.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Implements <see cref="ICliHandler"/> to create a new organization profile.</item>
/// <item>Uses <see cref="IAuthService"/> to fetch an authenticated session for the provider.</item>
/// <item>Saves new <see cref="OrganizationProfile"/> in <see cref="IProfileStore{OrganizationProfile}"/>.</item>
/// <item>Displays a confirmation panel with created organization details.</item>
/// </list>
/// </remarks>
[AutoRegister(ServiceLifetime.Transient, typeof(OrgCreateCommandHandler))]
internal sealed class OrgCreateCommandHandler(
    IAuthService auth,
    IProfileStore<OrganizationProfile> store) : ICliHandler
{
    /// <summary>
    /// Executes 'organization create' command asynchronously.
    /// </summary>
    /// <param name="parseResult">Parsed CLI arguments.</param>
    /// <param name="ct">Cancellation token.</param>
    public async Task HandleAsync(ParseResult parseResult, CancellationToken ct)
    {
        var name = parseResult.GetValue<string>("name");
        var provider = parseResult.GetValue<string>("--provider");

        if (string.IsNullOrWhiteSpace(name))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] name is required.");
            return;
        }

        if (string.IsNullOrWhiteSpace(provider))
        {
            AnsiConsole.MarkupLine("[red]Error:[/] --provider is required.");
            return;
        }

        try
        {
            await AnsiConsole.Status()
                .Spinner(Spinner.Known.Dots)
                .SpinnerStyle(Style.Parse("blue"))
                .StartAsync($"Creating organization [bold]{name}[/]...", async ctx =>
                {
                    var session = await auth.FetchSession(provider, ct);

                    var profile = new OrganizationProfile
                    {
                        Id = Ulid.NewUlid(),
                        Name = name,
                        Provider = provider,
                        CreatedAt = DateTime.UtcNow,
                        SessionId = session.Id
                    };

                    await store.SaveAsync(profile, ct);

                    ctx.Status("Finalizing...");
                    await Task.Delay(150, ct);
                });

            DisplayConfirmation(name, provider);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red] Failed to create organization: {ex.Message}[/]");
        }
    }

    private static void DisplayConfirmation(string name, string provider)
    {
        var panel = new Panel(
                $"[bold]Name:[/] {name}\n" +
                $"[bold]Provider:[/] {provider}"
            )
            .Header("[green]Organization Created[/]")
            .Border(BoxBorder.Rounded)
            .BorderStyle(Color.Green)
            .Padding(new Padding(1));

        AnsiConsole.Write(panel);
    }
}