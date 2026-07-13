using System.Net;
using System.Text.Encodings.Web;
using AwesomeAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.Integration.Test.Fixtures;
using RESTyard.Schema.Model;
using Xunit.Abstractions;

namespace RESTyard.Integration.Test;

public class GuideEndpointTests : IAsyncLifetime
{
    // Literal on purpose: pins the wire value of RESTyard.MediaTypes.GuideMediaType
    // (the type is compiled into both RESTyard.AspNetCore and RESTyard.Client, so direct use is ambiguous here).
    private const string GuideMediaType = "text/vnd.restyard.api-guide+markdown";

    private readonly CarShackWaf waf;

    public GuideEndpointTests(ITestOutputHelper outputHelper)
    {
        waf = new CarShackWaf(outputHelper);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await waf.DisposeAsync();

    [Fact]
    public async Task Guide_endpoint_serves_markdown_file_with_correct_content_type()
    {
        var client = waf.CreateClient();
        var response = await client.GetAsync("/api-guide");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be(GuideMediaType);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("CarShack API Guide");
    }

    [Fact]
    public async Task Guide_endpoint_emits_private_cache_header_when_max_age_set()
    {
        // CarShack maps the guide with CacheMaxAge = 5 minutes and default (Private) visibility
        var client = waf.CreateClient();
        var response = await client.GetAsync("/api-guide");

        var cacheControl = response.Headers.CacheControl;
        cacheControl.Should().NotBeNull();
        cacheControl!.Private.Should().BeTrue();
        cacheControl.Public.Should().BeFalse();
        cacheControl.MaxAge.Should().Be(TimeSpan.FromMinutes(5));
    }

    [Fact]
    public async Task Entrypoint_advertises_apiGuide_link_that_resolves_to_guide()
    {
        var client = waf.CreateClient();
        var entrypointJson = await client.GetStringAsync("/EntryPoint");

        entrypointJson.Should().Contain("api-guide");
        entrypointJson.Should().Contain("/api-guide");

        var response = await client.GetAsync("/api-guide");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Schema_endpoint_has_no_cache_header_by_default()
    {
        // CarShack maps the schema endpoints without CacheMaxAge
        var client = waf.CreateClient();

        var schemaResponse = await client.GetAsync("/hypermedia-schema");
        schemaResponse.Headers.CacheControl.Should().BeNull();

        var accessGroupsResponse = await client.GetAsync("/schema/access-groups");
        accessGroupsResponse.Headers.CacheControl.Should().BeNull();
    }

    [Fact]
    public async Task Guide_provider_overload_serves_content_without_cache_header_by_default()
    {
        using var server = await StartTestServer(endpoints =>
            endpoints.MapApiGuide(new TestGuideProvider("# From provider")));
        var client = server.CreateClient();

        var response = await client.GetAsync("/api-guide");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be(GuideMediaType);
        (await response.Content.ReadAsStringAsync()).Should().Be("# From provider");
        response.Headers.CacheControl.Should().BeNull();
    }

    [Fact]
    public async Task Guide_endpoint_emits_public_cache_header_when_visibility_public()
    {
        using var server = await StartTestServer(endpoints =>
            endpoints.MapApiGuide(new TestGuideProvider("# Guide"), o =>
            {
                o.CacheMaxAge = TimeSpan.FromMinutes(10);
                o.CacheVisibility = CacheVisibility.Public;
            }));
        var client = server.CreateClient();

        var response = await client.GetAsync("/api-guide");

        var cacheControl = response.Headers.CacheControl;
        cacheControl.Should().NotBeNull();
        cacheControl!.Public.Should().BeTrue();
        cacheControl.Private.Should().BeFalse();
        cacheControl.MaxAge.Should().Be(TimeSpan.FromMinutes(10));
    }

    [Fact]
    public async Task Schema_endpoint_emits_public_cache_header_when_visibility_public()
    {
        using var server = await StartTestServer(
            endpoints => endpoints.MapHypermediaSchema(o =>
            {
                o.CacheMaxAge = TimeSpan.FromSeconds(30);
                o.CacheVisibility = CacheVisibility.Public;
            }),
            services => services.AddSingleton(new HypermediaApiSchema()));
        var client = server.CreateClient();

        var response = await client.GetAsync("/hypermedia-schema");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cacheControl = response.Headers.CacheControl;
        cacheControl.Should().NotBeNull();
        cacheControl!.Public.Should().BeTrue();
        cacheControl.MaxAge.Should().Be(TimeSpan.FromSeconds(30));
    }

    [Fact]
    public async Task AccessGroups_endpoint_is_always_private_when_max_age_set()
    {
        // HypermediaSchemaAccessGroupsOptions has no CacheVisibility option at all —
        // the response can vary per caller, so public caching is structurally impossible.
        using var server = await StartTestServer(
            endpoints => endpoints.MapHypermediaSchemaAccessGroups(o =>
                o.CacheMaxAge = TimeSpan.FromMinutes(1)),
            services => services.AddSingleton(new HypermediaApiSchema()));
        var client = server.CreateClient();

        var response = await client.GetAsync("/schema/access-groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var cacheControl = response.Headers.CacheControl;
        cacheControl.Should().NotBeNull();
        cacheControl!.Private.Should().BeTrue();
        cacheControl.Public.Should().BeFalse();
    }

    [Fact]
    public async Task Guide_endpoint_with_RequireAuthorization_returns_401_for_anonymous_caller()
    {
        using var server = await StartTestServer(
            endpoints => endpoints
                .MapApiGuide(new TestGuideProvider("# Secret guide"))
                .RequireAuthorization(),
            services =>
            {
                services
                    .AddAuthentication("Test")
                    .AddScheme<AuthenticationSchemeOptions, AnonymousAuthenticationHandler>("Test", null);
                services.AddAuthorization();
            },
            useAuth: true);
        var client = server.CreateClient();

        var response = await client.GetAsync("/api-guide");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Guide_file_overload_fails_fast_when_file_is_missing()
    {
        var start = () => StartTestServer(endpoints =>
            endpoints.MapApiGuide("does-not-exist.md"));

        await start.Should().ThrowAsync<FileNotFoundException>()
            .WithMessage("*does-not-exist.md*");
    }

    private static async Task<TestServer> StartTestServer(
        Action<IEndpointRouteBuilder> mapEndpoints,
        Action<IServiceCollection>? configureServices = null,
        bool useAuth = false)
    {
        var host = await new HostBuilder()
            .ConfigureWebHost(webBuilder => webBuilder
                .UseTestServer()
                .ConfigureServices(services =>
                {
                    services.AddRouting();
                    configureServices?.Invoke(services);
                })
                .Configure(app =>
                {
                    app.UseRouting();
                    if (useAuth)
                    {
                        app.UseAuthentication();
                        app.UseAuthorization();
                    }

                    app.UseEndpoints(mapEndpoints);
                }))
            .StartAsync();
        return host.GetTestServer();
    }

    private sealed class TestGuideProvider(string content) : IApiGuideProvider
    {
        public Task<string> GetGuideAsync(HttpContext context) => Task.FromResult(content);
    }

    private sealed class AnonymousAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
    {
        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
            => Task.FromResult(AuthenticateResult.NoResult());
    }
}
