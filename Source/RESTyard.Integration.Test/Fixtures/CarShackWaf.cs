using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Xunit.Abstractions;

namespace RESTyard.Integration.Test.Fixtures;

public class CarShackWaf : WebApplicationFactory<CarShack.Program>, IAsyncLifetime
{
    private readonly ITestOutputHelper testOutputHelper;
    public const string BaseUrl = "http://localhost:5000";

    public CarShackWaf(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseUrls(BaseUrl);

        builder.ConfigureTestServices(
            services =>
            {
                services
                    .RemoveAll<ILoggerFactory>()
                    .AddSingleton<ILoggerFactory>(sp => new LoggerFactoryMock(this.testOutputHelper));
            });
    }

    public Task InitializeAsync()
    {
        return Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await this.DisposeAsync();
    }

    private class LoggerFactoryMock(ITestOutputHelper outputHelper) : ILoggerFactory
    {
        public void Dispose()
        {
            
        }

        public void AddProvider(ILoggerProvider provider)
        {
            
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new LoggerMock(outputHelper);
        }

        private class LoggerMock(ITestOutputHelper output) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull
            {
                return null;
            }

            public bool IsEnabled(LogLevel logLevel)
            {
                return true;
            }

            public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            {
                output.WriteLine(formatter(state, exception));
            }
        }
    }
}

public static class ISCExtensions
{
    public static IServiceCollection RemoveAll<T>(this IServiceCollection services)
    {
        return services.RemoveAll(typeof(T));
    }

    public static IServiceCollection RemoveAll(this IServiceCollection services, Type type)
    {
        for (int i = services.Count - 1; i >= 0; i--)
        {
            ServiceDescriptor? descriptor = services[i];
            if (descriptor.ServiceType == type && descriptor.ServiceKey == null)
            {
                services.RemoveAt(i);
            }
        }

        return services;
    }
}