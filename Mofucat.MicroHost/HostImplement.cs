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

    public IServiceProvider Services => serviceProvider;

    public HostImplement(string[] args, IServiceProvider serviceProvider, IConfigurationRoot configuration, IHostEnvironment environment)
    {
        this.args = args;
        this.serviceProvider = serviceProvider;
        this.configuration = configuration;
        this.environment = environment;
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

        if (configuration is IConfigurationBuilder configurationBuilder)
        {
            foreach (var fileProvider in configurationBuilder.Sources.OfType<FileConfigurationSource>().Select(static x => x.FileProvider).Distinct())
            {
                (fileProvider as IDisposable)?.Dispose();
            }
        }

        (configuration as IDisposable)?.Dispose();
        (environment.ContentRootFileProvider as IDisposable)?.Dispose();
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
