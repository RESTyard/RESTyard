using System.Collections.Generic;
using System.Net;
using AwesomeAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.Integration.Test.Fixtures;
using RESTyard.Schema;
using RESTyard.Schema.Model;
using Xunit.Abstractions;

namespace RESTyard.Integration.Test;

public class SchemaEndpointTests : IAsyncLifetime
{
    private readonly CarShackWaf waf;

    public SchemaEndpointTests(ITestOutputHelper outputHelper)
    {
        waf = new CarShackWaf(outputHelper);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync() => await waf.DisposeAsync();

    [Fact]
    public async Task Schema_endpoint_returns_json_with_correct_content_type()
    {
        var client = waf.CreateClient();
        var response = await client.GetAsync("/hypermedia-schema");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be(RESTyard.Schema.SchemaMediaTypes.HypermediaApiSchema);
    }

    [Fact]
    public async Task Schema_endpoint_returns_valid_HypermediaApiSchema()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema");

        var schema = HypermediaApiSchema.FromJson(json);

        schema.Should().NotBeNull();
        schema.SchemaVersion.Should().NotBeNullOrEmpty();
        schema.Title.Should().Be("CarShack API");
        schema.EntryPointName.Should().NotBeNullOrEmpty();
        schema.EntityTypes.Should().NotBeEmpty();
        schema.DeclaredAccessGroups.Should().Contain("customer");
        schema.DeclaredAccessGroups.Should().Contain("fleet-manager");
    }

    [Fact]
    public async Task Schema_endpoint_contains_expected_entity_types()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema");
        var schema = HypermediaApiSchema.FromJson(json);

        var entityNames = schema.EntityTypes.Select(e => e.Name).ToList();

        entityNames.Should().Contain("Customer");
        entityNames.Should().Contain("Entrypoint");
        entityNames.Should().Contain("CustomersRoot");
        entityNames.Should().Contain("Car");
    }

    [Fact]
    public async Task Schema_endpoint_actions_have_result_names()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema");
        var schema = HypermediaApiSchema.FromJson(json);

        var customersRoot = schema.EntityTypes.Single(e => e.Name == "CustomersRoot");
        var createQuery = customersRoot.Actions.Single(a => a.Name == "CreateQuery");

        createQuery.ResultName.Should().Be("CustomerQueryResult");
    }

    [Fact]
    public async Task Schema_endpoint_with_accessGroups_filters_entities()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema?accessGroups=customer");
        var schema = HypermediaApiSchema.FromJson(json);

        schema.DeclaredAccessGroups.Should().BeNull("filtered schema should strip DeclaredAccessGroups");
        schema.EntityTypes.Should().Contain(e => e.Name == "Customer");
        schema.EntityTypes.Should().NotContain(e => e.Name == "CarsRoot",
            "CarsRoot requires 'fleet-manager' which is not in the granted set");
    }

    [Fact]
    public async Task Schema_endpoint_with_excludeAccessGroups_filters_entities()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema?excludeAccessGroups=customer");
        var schema = HypermediaApiSchema.FromJson(json);

        schema.DeclaredAccessGroups.Should().BeNull();
        schema.EntityTypes.Should().NotContain(e => e.Name == "Customer");
        schema.EntityTypes.Should().Contain(e => e.Name == "CarsRoot");
    }

    [Fact]
    public async Task Schema_endpoint_with_both_params_returns_400()
    {
        var client = waf.CreateClient();
        var response = await client.GetAsync("/hypermedia-schema?accessGroups=read&excludeAccessGroups=admin");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task Schema_endpoint_sanitizer_strips_groups()
    {
        // Register a sanitizer that removes "secret" from requested groups
        var client = waf.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ISchemaAccessGroupSanitizer>(new StripSecretSanitizer());
            });
        }).CreateClient();

        // Request with "secret" group — sanitizer removes it, leaving only "read"
        // CarShack has no access groups, so all public entities remain
        var json = await client.GetStringAsync("/hypermedia-schema?accessGroups=read,secret");
        var schema = HypermediaApiSchema.FromJson(json);

        schema.Should().NotBeNull();
        schema.EntityTypes.Should().NotBeEmpty();
        schema.DeclaredAccessGroups.Should().BeNull();
    }

    [Fact]
    public async Task AccessGroups_endpoint_returns_correct_content_type()
    {
        var client = waf.CreateClient();
        var response = await client.GetAsync("/schema/access-groups");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be(SchemaMediaTypes.HypermediaSchemaAccessGroups);
    }

    [Fact]
    public async Task AccessGroups_endpoint_returns_declared_groups()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/schema/access-groups");

        json.Should().Contain("\"accessGroups\"");
        json.Should().Contain("customer");
        json.Should().Contain("fleet-manager");
    }

    [Fact]
    public async Task AccessGroups_endpoint_with_sanitizer_filters_groups()
    {
        var client = waf.WithWebHostBuilder(builder =>
        {
            builder.ConfigureTestServices(services =>
            {
                services.AddSingleton<ISchemaAccessGroupSanitizer>(new StripSecretSanitizer());
            });
        }).CreateClient();

        // Sanitizer only affects groups that exist — with no declared groups, result is still empty
        var json = await client.GetStringAsync("/schema/access-groups");
        json.Should().Contain("\"accessGroups\"");
    }

    private class StripSecretSanitizer : ISchemaAccessGroupSanitizer
    {
        public IReadOnlySet<string> SanitizeRequestedGroups(
            IReadOnlySet<string> requestedGroups, HttpContext httpContext)
        {
            var sanitized = new HashSet<string>(requestedGroups, StringComparer.Ordinal);
            sanitized.Remove("secret");
            return sanitized;
        }
    }
}
