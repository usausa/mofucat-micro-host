namespace Mofucat.MicroHost;

using System.Diagnostics.CodeAnalysis;

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

    [RequiresDynamicCode("Uses Microsoft.Extensions.DependencyInjection which requires dynamic code.")]
    [RequiresUnreferencedCode("Uses Microsoft.Extensions.DependencyInjection which requires unreferenced code.")]
    void ConfigureContainer<TContainerBuilder>(
        IServiceProviderFactory<TContainerBuilder> factory,
        Action<TContainerBuilder>? configure = null)
        where TContainerBuilder : notnull;

    [RequiresDynamicCode("Uses Microsoft.Extensions.DependencyInjection which requires dynamic code.")]
    [RequiresUnreferencedCode("Uses Microsoft.Extensions.DependencyInjection which requires unreferenced code.")]
    IHost Build();
}
