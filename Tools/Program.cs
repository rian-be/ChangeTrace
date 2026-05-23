using ChangeTrace.Tools.Commands;
using ChangeTrace.Tools.Commands.Assets;
using ChangeTrace.Tools.Commands.Dev;
using ChangeTrace.Tools.Commands.Publish;
using ChangeTrace.Tools.Infrastructure;
using ChangeTrace.Tools.Infrastructure.FileSystem;
using ChangeTrace.Tools.Infrastructure.Processes;
using ChangeTrace.Tools.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var services = new ServiceCollection();

services.AddSingleton<IFileSystem, FileSystem>();
services.AddSingleton<IRepositoryRootFinder, RepositoryRootFinder>();
services.AddSingleton<IProcessRunner, ProcessRunner>();

services.AddTransient<BuildLanguageIconAtlasCommand>();
services.AddTransient<ValidateAssetsCommand>();
services.AddTransient<PublishCommand>();
services.AddTransient<CleanCommand>();

await using var provider = services.BuildServiceProvider();

AnsiConsole.Write(
    new FigletText("ChangeTrace Tools")
        .Color(Color.Green));

try
{
    var command = ResolveCommand(args, provider);

    if (command is null)
        return 0;

    return await command.ExecuteAsync();
}
catch (Exception ex)
{
    AnsiConsole.MarkupLine($"[red]Error:[/] {ex.Message}");
    return 1;
}

static ICommand? ResolveCommand(
    string[] args,
    IServiceProvider services)
{
    if (args.Length > 0)
    {
        var commandName = string.Join(
            " ",
            args.Select(x => x.ToLowerInvariant()));

        return commandName switch
        {
            "assets atlas" => services.GetRequiredService<BuildLanguageIconAtlasCommand>(),
            "atlas" => services.GetRequiredService<BuildLanguageIconAtlasCommand>(),

            "assets validate" => services.GetRequiredService<ValidateAssetsCommand>(),
            "validate assets" => services.GetRequiredService<ValidateAssetsCommand>(),

            "publish" => services.GetRequiredService<PublishCommand>(),
            "build" => services.GetRequiredService<PublishCommand>(),

            "clean" => services.GetRequiredService<CleanCommand>(),
            "dev clean" => services.GetRequiredService<CleanCommand>(),

            "exit" => null,
            _ => ShowUnknownCommand(commandName)
        };
    }

    var selected = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("What do you want to do?")
            .AddChoices(
                "Assets / Build Language Icon Atlas",
                "Assets / Validate Assets",
                "Publish",
                "Clean",
                "Exit"));

    return selected switch
    {
        "Assets / Build Language Icon Atlas" => services.GetRequiredService<BuildLanguageIconAtlasCommand>(),
        "Assets / Validate Assets" => services.GetRequiredService<ValidateAssetsCommand>(),
        "Publish" => services.GetRequiredService<PublishCommand>(),
        "Clean" => services.GetRequiredService<CleanCommand>(),
        _ => null
    };
}

static ICommand? ShowUnknownCommand(string commandName)
{
    AnsiConsole.MarkupLine($"[red]Unknown command:[/] {commandName}");
    AnsiConsole.WriteLine();

    AnsiConsole.MarkupLine("[yellow]Available commands:[/]");
    AnsiConsole.WriteLine("  assets atlas");
    AnsiConsole.WriteLine("  assets validate");
    AnsiConsole.WriteLine("  publish");
    AnsiConsole.WriteLine("  clean");

    return null;
}