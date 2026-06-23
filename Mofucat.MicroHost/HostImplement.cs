namespace Mofucat.MicroHost;

using System.Runtime.InteropServices;

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
        using var cts = new CancellationTokenSource();
        // ReSharper disable AccessToDisposedClosure
        using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, context => HandleSignal(context, cts));
        using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => HandleSignal(context, cts));
        // ReSharper restore AccessToDisposedClosure

        try
        {
            foreach (var runner in serviceProvider.GetServices<IHostRunner>())
            {
                await runner.RunAsync(args, cts.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Canceled by SIGINT/SIGTERM
        }
    }

    private static void HandleSignal(PosixSignalContext context, CancellationTokenSource cancellationTokenSource)
    {
        context.Cancel = !cancellationTokenSource.IsCancellationRequested;
        cancellationTokenSource.Cancel();
    }
}
#pragma warning restore IDE0032
