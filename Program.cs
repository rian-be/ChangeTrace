using System.CommandLine;
using ChangeTrace.Cli.Extensions;
using ChangeTrace.Cli.Handlers;
using ChangeTrace.Cli.Interfaces;
using ChangeTrace.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ChangeTrace;

public static class Program
{ 
    public static async Task<int> Main(string[] args)
    {
        var services = new ServiceCollection();
     //   services.ConfigureApp(logLevel: LogLevel.Information);
        services.ConfigureApp(logLevel: LogLevel.Debug);

        services.AddTransient<ExportCommandHandler>();
        
        var provider = services.BuildServiceProvider();

        var root = new RootCommand("ChangeTrace - repository timeline generator");
        foreach (var def in provider.GetServices<ICliCommand>())
        {
            var cmd = def.Build();
            cmd.AttachHandler(provider, def.HandlerType);
            root.Add(cmd);
        }

        return await root.Parse(args).InvokeAsync();
    }
}