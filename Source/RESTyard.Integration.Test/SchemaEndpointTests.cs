using System.Net;
using AwesomeAssertions;
using RESTyard.Integration.Test.Fixtures;
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
    public async Task Schema_endpoint_with_accessGroups_returns_filtered_schema()
    {
        var client = waf.CreateClient();
        // CarShack has no access groups, so filtering with any group should still return all public entities
        var json = await client.GetStringAsync("/hypermedia-schema?accessGroups=read");
        var schema = HypermediaApiSchema.FromJson(json);

        schema.Should().NotBeNull();
        schema.DeclaredAccessGroups.Should().BeNull("filtered schema should strip DeclaredAccessGroups");
        schema.EntityTypes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Schema_endpoint_with_excludeAccessGroups_returns_filtered_schema()
    {
        var client = waf.CreateClient();
        var json = await client.GetStringAsync("/hypermedia-schema?excludeAccessGroups=admin");
        var schema = HypermediaApiSchema.FromJson(json);

        schema.Should().NotBeNull();
        schema.DeclaredAccessGroups.Should().BeNull();
        schema.EntityTypes.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Schema_endpoint_with_both_params_returns_400()
    {
        var client = waf.CreateClient();
        var response = await client.GetAsync("/hypermedia-schema?accessGroups=read&excludeAccessGroups=admin");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/problem+json");
    }
}
