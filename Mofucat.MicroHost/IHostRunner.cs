namespace Mofucat.MicroHost;

internal interface IHostRunner
{
    ValueTask RunAsync(string[] args);
}
