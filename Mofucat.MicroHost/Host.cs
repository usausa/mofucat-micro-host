namespace Mofucat.MicroHost;

using System.Diagnostics.CodeAnalysis;

public static class Host
{
    [RequiresDynamicCode("Uses Microsoft.Extensions.DependencyInjection which requires dynamic code.")]
    [RequiresUnreferencedCode("Uses Microsoft.Extensions.DependencyInjection which requires unreferenced code.")]
    public static IHostBuilder CreateBuilder(HostBuilderSettings? setting = null) =>
        new HostBuilder(setting);

    [RequiresDynamicCode("Uses Microsoft.Extensions.DependencyInjection which requires dynamic code.")]
    [RequiresUnreferencedCode("Uses Microsoft.Extensions.DependencyInjection which requires unreferenced code.")]
    public static IHostBuilder CreateBuilder(string[] args) =>
        new HostBuilder(new HostBuilderSettings { Args = args });
}
