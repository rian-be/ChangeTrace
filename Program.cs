using ChangeTrace.Cli.Extensions;
using ChangeTrace.Configuration;
using dotenv.net;
using ChangeTrace.Rendering.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTK.Windowing.Desktop;

namespace ChangeTrace;

public static class Program
{ 
    public static async Task<int> Main(string[] args)
    {
        GLFWProvider.CheckForMainThread = false;
        DotEnv.Fluent()
            .WithoutExceptions()
            .WithoutOverwriteExistingVars()
            .WithEnvFiles(
                ".env",
                Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
                    ".changetrace",
                    ".env"))
            .Load();

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
