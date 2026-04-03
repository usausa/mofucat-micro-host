#pragma warning disable IDE0210
#pragma warning disable CA1812
#pragma warning disable CA1848
#pragma warning disable CA1873
namespace Example;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using Mofucat.MicroHost;

using Serilog;

internal static class Program
{
    private static async Task Main(string[] args)
    {
        // HostBuilder
        var builder = Host.CreateBuilder(args);

        // Logging
        builder.Logging.ClearProviders();
        builder.Services.AddSerilog(options =>
        {
            options.ReadFrom.Configuration(builder.Configuration);
        });

        // Setting
        builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));

        // Application
        builder.UseFramework<FrameworkApplication>();

        // Build
#pragma warning disable CA2007
        await using var host = builder.Build();
#pragma warning restore CA2007

        // Run
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

    private readonly Settings settings;

    public FrameworkApplication(ILogger<FrameworkApplication> log, IOptions<Settings> settings)
    {
        this.log = log;
        this.settings = settings.Value;
    }

    public void Run()
    {
        log.LogInformation("Hello, {Value}!", settings.Value);
    }
}

internal sealed class Settings
{
    public string Value { get; set; } = default!;
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
