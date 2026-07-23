namespace Mofucat.MicroHost;

public interface IHost : IAsyncDisposable
{
    IServiceProvider Services { get; }

    ValueTask RunAsync(CancellationToken cancellationToken = default);
}
