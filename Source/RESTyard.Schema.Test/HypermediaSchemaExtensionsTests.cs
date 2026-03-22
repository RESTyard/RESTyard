using System;
using AwesomeAssertions;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.Schema.Model;
using RESTyard.Schema.SchemaGeneration;
using Xunit;

namespace RESTyard.Schema.Test;

public class HypermediaSchemaExtensionsTests
{
    [Fact]
    public void AddHypermediaSchema_registers_schema_singleton()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJsonSchemaFactory, JsonSchemaFactory>();
        services.AddHypermediaSchema(o =>
        {
            o.Title = "Test";
            o.EntryPointName = "Root";
        });

        var sp = services.BuildServiceProvider();
        var schema = sp.GetRequiredService<HypermediaApiSchema>();

        schema.Title.Should().Be("Test");
        schema.EntryPointName.Should().Be("Root");
    }

    [Fact]
    public void AddHypermediaSchema_without_IJsonSchemaFactory_throws_on_resolve()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddHypermediaSchema();

        var sp = services.BuildServiceProvider();

        var act = () => sp.GetRequiredService<HypermediaApiSchema>();
        act.Should().Throw<InvalidOperationException>();
    }
}
