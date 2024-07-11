using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;

namespace RESTyard.Integration.Test.Fixtures;

public class CarShackWaf : WebApplicationFactory<CarShack.Program>, IAsyncLifetime
{
    public const string BaseUrl = "http://localhost:5000";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.UseUrls(BaseUrl);

        builder.ConfigureTestServices(
            services =>
            {

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
}