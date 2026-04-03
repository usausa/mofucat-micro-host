namespace Mofucat.MicroHost;

using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

internal sealed class HostEnvironment : IHostEnvironment
{
    public string ApplicationName { get; set; } = default!;

    public string EnvironmentName { get; set; } = default!;

    public string ContentRootPath { get; set; } = default!;

    public IFileProvider ContentRootFileProvider { get; set; } = default!;
}
