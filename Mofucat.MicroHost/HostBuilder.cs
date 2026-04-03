namespace Mofucat.MicroHost;

using System.Reflection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0032
#pragma warning disable CA1001
internal sealed class HostBuilder : IHostBuilder
{
    private readonly string[] args;

    private readonly ServiceCollection services = [];

    private readonly ConfigurationManager configuration;

    private readonly HostEnvironment environment;

    private readonly LoggingBuilder loggingBuilder;

    private Func<IServiceProvider> createServiceProvider;

    private Action<object> configureContainer = _ => { };

    public ConfigurationManager Configuration => configuration;

    public IHostEnvironment Environment => environment;

    public IServiceCollection Services => services;

    public ILoggingBuilder Logging => loggingBuilder;

    public HostBuilder(HostBuilderSettings? settings)
    {
        args = settings?.Args ?? [];

        // Default service provider factory
        createServiceProvider = () =>
        {
            configureContainer(Services);
            return Services.BuildServiceProvider();
        };

        // Environment
        var contentRootPath = settings?.ContentRootPath ?? AppContext.BaseDirectory;
        environment = new HostEnvironment
        {
            ApplicationName = settings?.ApplicationName ?? Assembly.GetEntryAssembly()?.GetName().Name ?? string.Empty,
            EnvironmentName = settings?.EnvironmentName ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production",
            ContentRootPath = contentRootPath,
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath)
        };

        // Configuration
        configuration = new ConfigurationManager();

        // Logging
        loggingBuilder = new LoggingBuilder(services);

        // Add basic services
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(environment);

        // Default
        if (!settings?.DisableDefaults ?? true)
        {
            configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
            configuration.AddJsonFile($"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: true);
            configuration.AddEnvironmentVariables();

            services.AddLogging(logging =>
            {
                var loggingSection = configuration.GetSection("Logging");
                if (loggingSection.Exists())
                {
                    logging.AddConfiguration(loggingSection);
                }
                logging.AddConsole();
            });
        }
    }

    public void ConfigureContainer<TContainerBuilder>(IServiceProviderFactory<TContainerBuilder> factory, Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull
    {
        createServiceProvider = () =>
        {
            var containerBuilder = factory.CreateBuilder(Services);
            configureContainer(containerBuilder);
            return factory.CreateServiceProvider(containerBuilder);
        };

        configureContainer = containerBuilder => configure?.Invoke((TContainerBuilder)containerBuilder);
    }

    public IHost Build()
    {
        var serviceProvider = createServiceProvider();

        return new HostImplement(args, serviceProvider);
    }
}
#pragma warning restore CA1001
#pragma warning restore IDE0032
