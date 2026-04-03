namespace Mofucat.MicroHost;

using Microsoft.Extensions.Configuration;

#pragma warning disable CA1819
public sealed class HostBuilderSettings
{
    public bool DisableDefaults { get; set; }

    public string[]? Args { get; set; }

    public ConfigurationManager? Configuration { get; set; }

    public string? EnvironmentName { get; set; }

    public string? ApplicationName { get; set; }

    public string? ContentRootPath { get; set; }
}
#pragma warning restore CA1819
