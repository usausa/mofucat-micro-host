namespace Mofucat.MicroHost;

using System.Diagnostics.CodeAnalysis;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

#pragma warning disable IDE0032
#pragma warning disable CA1001
[RequiresDynamicCode("Uses Microsoft.Extensions.DependencyInjection which requires dynamic code.")]
[RequiresUnreferencedCode("Uses Microsoft.Extensions.DependencyInjection which requires unreferenced code.")]
internal sealed class HostBuilder : IHostBuilder
{
    private readonly string[] args;

    private readonly ServiceCollection services = [];

    private readonly ConfigurationManager configuration;

    private readonly bool ownsConfiguration;

    private readonly HostEnvironment environment;

    private readonly LoggingBuilder loggingBuilder;

    private Func<IServiceProvider> createServiceProvider;

    private Action<object> configureContainer = _ => { };

    private bool built;

    public ConfigurationManager Configuration => configuration;

    public IHostEnvironment Environment => environment;

    public IServiceCollection Services => services;

    public ILoggingBuilder Logging => loggingBuilder;

    public HostBuilder(HostBuilderSettings? settings)
    {
        args = settings?.Args is { } a ? [.. a] : [];

        // Environment
        string contentRootPath;
        if (settings?.ContentRootPath is { } path)
        {
            if (String.IsNullOrWhiteSpace(path))
            {
                throw new ArgumentException("ContentRootPath must not be empty.", nameof(settings));
            }

            contentRootPath = Path.GetFullPath(path);
        }
        else
        {
            contentRootPath = AppContext.BaseDirectory;
        }

        environment = new HostEnvironment
        {
            ApplicationName = settings?.ApplicationName ?? AppDomain.CurrentDomain.FriendlyName,
            EnvironmentName = settings?.EnvironmentName ?? System.Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production",
            ContentRootPath = contentRootPath,
            ContentRootFileProvider = new PhysicalFileProvider(contentRootPath)
        };

        // Default service provider factory
        createServiceProvider = () =>
        {
            configureContainer(Services);
            return environment.IsDevelopment()
                ? Services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true })
                : Services.BuildServiceProvider();
        };

        // Configuration
        ownsConfiguration = settings?.Configuration is null;
        configuration = settings?.Configuration ?? new ConfigurationManager();

        // Logging
        loggingBuilder = new LoggingBuilder(services);

        // Add basic services
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(environment);

        // Default
        if (!settings?.DisableDefaults ?? true)
        {
            var reloadOnChange = settings?.ReloadConfigurationOnChange ?? true;
            configuration.AddJsonFile(environment.ContentRootFileProvider, "appsettings.json", optional: true, reloadOnChange: reloadOnChange);
            configuration.AddJsonFile(environment.ContentRootFileProvider, $"appsettings.{environment.EnvironmentName}.json", optional: true, reloadOnChange: reloadOnChange);
            configuration.AddEnvironmentVariables();
            if (args.Length > 0)
            {
                configuration.AddCommandLine(args);
            }

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
        if (built)
        {
            throw new InvalidOperationException("Build can only be called once.");
        }

        built = true;

        var serviceProvider = createServiceProvider();

        return new HostImplement(args, serviceProvider, configuration, environment, ownsConfiguration);
    }
}
#pragma warning restore CA1001
#pragma warning restore IDE0032
