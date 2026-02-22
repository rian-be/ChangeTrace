using System.CommandLine;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.CredentialTrace.Interfaces;
using ChangeTrace.CredentialTrace.Profiles;
using ChangeTrace.CredentialTrace.Services;
using Spectre.Console;

namespace ChangeTrace.Cli.Handlers.Profiles.Organizations;

internal sealed class OrgCreateCommandHandler(
    IAuthService auth,
    IProfileStore<OrganizationProfile> store) : ICliHandler
{
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