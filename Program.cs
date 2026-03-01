using ChangeTrace.Cli.Extensions;
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
        services.AddHttpClient();

        var provider = services.BuildServiceProvider();
      
        var root = CliComposer.Build(provider);
     //   CliComposer.Dump(root);
        
        return await root.Parse(args).InvokeAsync();
    }
}