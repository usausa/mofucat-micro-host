namespace Example;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Mofucat.MicroHost;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateBuilder(args);

        // TODO custom logging, setting

        builder.UseFramework<FrameworkApplication>();

        await using var host = builder.Build();

        await host.RunAsync().ConfigureAwait(false);
    }
}

//--------------------------------------------------------------------------------
// Framework
//--------------------------------------------------------------------------------

internal interface IApplication
{
    void Run();
}

internal sealed class FrameworkApplication : IApplication
{
    private readonly ILogger<FrameworkApplication> log;

    public FrameworkApplication(ILogger<FrameworkApplication> log)
    {
        this.log = log;
    }

    public void Run()
    {
        log.LogInformation("Hello, World!");
    }
}

//--------------------------------------------------------------------------------
// Bridge
//--------------------------------------------------------------------------------

internal static class Extensions
{
    public static IHostBuilder UseFramework<TApplication>(this IHostBuilder builder)
        where TApplication : class, IApplication
    {
        builder.Services.AddSingleton<IApplication, TApplication>();
        builder.Services.AddSingleton<IHostRunner, FrameworkRunner>();
        return builder;
    }
}

internal sealed class FrameworkRunner : IHostRunner
{
    private readonly IApplication app;

    public FrameworkRunner(IApplication app)
    {
        this.app = app;
    }

    public ValueTask RunAsync(string[] args)
    {
        app.Run();
        return ValueTask.CompletedTask;
    }
}
