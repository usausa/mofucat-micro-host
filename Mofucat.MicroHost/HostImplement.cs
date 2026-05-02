namespace Mofucat.MicroHost;

using Microsoft.Extensions.DependencyInjection;

#pragma warning disable IDE0032
internal sealed class HostImplement : IHost
{
    private readonly string[] args;

    private readonly IServiceProvider serviceProvider;

    public IServiceProvider Services => serviceProvider;

    public HostImplement(string[] args, IServiceProvider serviceProvider)
    {
        this.args = args;
        this.serviceProvider = serviceProvider;
    }

    public ValueTask DisposeAsync()
    {
        if (serviceProvider is IAsyncDisposable asyncDisposable)
        {
            return asyncDisposable.DisposeAsync();
        }
        if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }
        return ValueTask.CompletedTask;
    }

    public async ValueTask RunAsync()
    {
        foreach (var arg in serviceProvider.GetServices<IHostRunner>())
        {
            await arg.RunAsync(args).ConfigureAwait(false);
        }
    }
}
#pragma warning restore IDE0032
