namespace Mofucat.MicroHost;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public interface IHostBuilder
{
    ConfigurationManager Configuration { get; }

    IHostEnvironment Environment { get; }

    IServiceCollection Services { get; }

    ILoggingBuilder Logging { get; }

    void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull;

    IHost Build();
}
