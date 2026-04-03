namespace Mofucat.MicroHost;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal sealed class LoggingBuilder : ILoggingBuilder
{
    public IServiceCollection Services { get; }

    public LoggingBuilder(IServiceCollection services)
    {
        Services = services;
    }
}
