# Mofucat.MicroHost

[![NuGet](https://img.shields.io/nuget/v/Mofucat.MicroHost.svg)](https://www.nuget.org/packages/Mofucat.MicroHost)

Lightweight host builder for .NET applications.

## Basic usage

Implement `IHostRunner` and register it with the DI container:

```csharp
var builder = Host.CreateBuilder(args);

builder.Services.AddSingleton<IHostRunner, MyRunner>();

await using var host = builder.Build();
await host.RunAsync();
```

```csharp
internal sealed class MyRunner : IHostRunner
{
    public ValueTask RunAsync(string[] args)
    {
        Console.WriteLine("Hello!");
        return ValueTask.CompletedTask;
    }
}
```
