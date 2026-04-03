namespace Mofucat.MicroHost;

public static class Host
{
    public static IHostBuilder CreateBuilder(HostBuilderSettings? setting = null) =>
        new HostBuilder(setting);

    public static IHostBuilder CreateBuilder(string[] args) =>
        new HostBuilder(new HostBuilderSettings { Args = args });
}
