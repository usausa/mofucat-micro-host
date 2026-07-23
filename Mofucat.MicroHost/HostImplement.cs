namespace Mofucat.MicroHost;

using System.Runtime.InteropServices;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

#pragma warning disable IDE0032
internal sealed class HostImplement : IHost
{
    private readonly string[] args;

    private readonly IServiceProvider serviceProvider;

    private readonly IConfigurationRoot configuration;

    private readonly IHostEnvironment environment;

    private readonly bool ownsConfiguration;

    public IServiceProvider Services => serviceProvider;

    public HostImplement(string[] args, IServiceProvider serviceProvider, IConfigurationRoot configuration, IHostEnvironment environment, bool ownsConfiguration)
    {
        this.args = args;
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.environment = environment;
        this.ownsConfiguration = ownsConfiguration;
    }

    public async ValueTask DisposeAsync()
    {
        if (serviceProvider is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else if (serviceProvider is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (ownsConfiguration)
        {
            (configuration as IDisposable)?.Dispose();
        }

        (environment.ContentRootFileProvider as IDisposable)?.Dispose();
    }

    public async ValueTask RunAsync(CancellationToken cancellationToken = default)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        // ReSharper disable AccessToDisposedClosure
        using var sigInt = PosixSignalRegistration.Create(PosixSignal.SIGINT, context => HandleSignal(context, cts));
        using var sigTerm = PosixSignalRegistration.Create(PosixSignal.SIGTERM, context => HandleSignal(context, cts));
        // ReSharper restore AccessToDisposedClosure

#pragma warning disable CA2025
        var tasks = serviceProvider.GetServices<IHostRunner>()
            .Select(x => RunRunnerAsync(x, cts))
            .ToArray();
#pragma warning restore CA2025
        try
        {
            await Task.WhenAll(tasks).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Ignore
        }
    }

    private async Task RunRunnerAsync(IHostRunner runner, CancellationTokenSource cts)
    {
        try
        {
            await runner.RunAsync(args, cts.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cts.IsCancellationRequested)
        {
            // Ignore
        }
        catch
        {
            await cts.CancelAsync().ConfigureAwait(false);
            throw;
        }
    }

    private static void HandleSignal(PosixSignalContext context, CancellationTokenSource cancellationTokenSource)
    {
        context.Cancel = !cancellationTokenSource.IsCancellationRequested;
        cancellationTokenSource.Cancel();
    }
}
#pragma warning restore IDE0032
