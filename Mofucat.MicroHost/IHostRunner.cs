namespace Mofucat.MicroHost;

public interface IHostRunner
{
    ValueTask RunAsync(string[] args);
}
