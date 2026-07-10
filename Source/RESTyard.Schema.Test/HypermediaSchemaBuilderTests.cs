using System.Collections.Generic;
using AwesomeAssertions;
using Microsoft.Extensions.Logging;
using RESTyard.Schema;
using RESTyard.Schema.Model;
using RESTyard.Schema.SchemaGeneration;
using Xunit;

namespace RESTyard.Schema.Test;

public class HypermediaSchemaBuilderTests
{
    [Fact]
    public void ComposeSchema_applies_options()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Customer", Classes = ["Customer"] },
            new() { Name = "Entrypoint", Classes = ["EntryPoint"] },
        };

        var options = new HypermediaSchemaOptions
        {
            Title = "Test API",
            Description = "A test API",
            ApiVersion = "2.0.0",
            EntryPointName = "Entrypoint",
            ExternalDocsUrl = "https://docs.example.com",
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, options, null);

        schema.Title.Should().Be("Test API");
        schema.Description.Should().Be("A test API");
        schema.ApiVersion.Should().Be("2.0.0");
        schema.EntryPointName.Should().Be("Entrypoint");
        schema.ExternalDocsUrl.Should().Be("https://docs.example.com");
        schema.EntityTypes.Should().HaveCount(2);
        schema.SchemaVersion.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public void ComposeSchema_auto_detects_entry_point()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Customer", Classes = ["Customer"] },
            new() { Name = "MyEntrypoint", Classes = ["EntryPoint", "Root"] },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, null);

        schema.EntryPointName.Should().Be("MyEntrypoint");
    }

    [Fact]
    public void ComposeSchema_entry_point_detection_is_case_insensitive()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Root", Classes = ["entrypoint"] },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, null);

        schema.EntryPointName.Should().Be("Root");
    }

    [Fact]
    public void ComposeSchema_no_entry_point_uses_empty_string()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Customer", Classes = ["Customer"] },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, null);

        schema.EntryPointName.Should().BeEmpty();
    }

    [Fact]
    public void DiscoverEntityTypes_logs_warning_when_no_registries_found()
    {
        var loggerFactory = LoggerFactory.Create(b => b.AddDebug());
        var logger = loggerFactory.CreateLogger(typeof(HypermediaSchemaBuilder));
        var factory = new JsonSchemaFactory();

        var entityTypes = HypermediaSchemaBuilder.DiscoverEntityTypes(factory, logger);

        entityTypes.Should().BeEmpty();
    }

    [Fact]
    public void ComposeSchema_applies_action_result_mappings()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new()
            {
                Name = "Root",
                Classes = ["Root"],
                Actions = [new ActionDescription { Name = "CreateQuery" }],
            },
        };

        var mappings = new List<ActionResultMapping>
        {
            new()
            {
                EntityName = "Root",
                ActionName = "CreateQuery",
                ResultName = "QueryResult",
                ResultClasses = ["QueryResult"],
            },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, mappings, null, null);

        var action = schema.EntityTypes[0].Actions[0];
        action.ResultName.Should().Be("QueryResult");
        action.ResultClasses.Should().BeEquivalentTo(["QueryResult"]);
    }

    [Fact]
    public void ComposeSchema_does_not_overwrite_existing_result_information()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new()
            {
                Name = "Root",
                Classes = ["Root"],
                Actions =
                [
                    new ActionDescription
                    {
                        Name = "CreateQuery",
                        ResultName = "LocalResult",
                        ResultClasses = ["LocalResult"],
                    },
                ],
            },
        };

        var mappings = new List<ActionResultMapping>
        {
            new() { EntityName = "Root", ActionName = "CreateQuery", ResultName = "OtherResult" },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, mappings, null, null);

        schema.EntityTypes[0].Actions[0].ResultName.Should().Be("LocalResult");
    }

    [Fact]
    public void ComposeSchema_ignores_unmatched_action_result_mappings()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Root", Classes = ["Root"] },
        };

        var mappings = new List<ActionResultMapping>
        {
            new() { EntityName = "Missing", ActionName = "Nope", ResultName = "QueryResult" },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, mappings, null, null);

        schema.EntityTypes.Should().ContainSingle();
    }

    [Fact]
    public void ComposeSchema_collects_DeclaredAccessGroups_from_all_levels()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new()
            {
                Name = "Customer",
                Classes = ["Customer"],
                AccessGroups = ["admin"],
                Actions = [new ActionDescription { Name = "Delete", AccessGroups = ["admin", "sales"] }],
                Links = [new LinkDescription { Relations = ["orders"], TargetName = "Order", AccessGroups = ["read"] }],
                EmbeddedEntities = [new EmbeddedEntityDescription { Relations = ["address"], TargetName = "Address", AccessGroups = ["read", "write"] }],
            },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, null);

        schema.DeclaredAccessGroups.Should().NotBeNull();
        schema.DeclaredAccessGroups.Should().BeEquivalentTo(["admin", "read", "sales", "write"]);
    }

    [Fact]
    public void ComposeSchema_returns_null_DeclaredAccessGroups_when_none_declared()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new() { Name = "Customer", Classes = ["Customer"] },
        };

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, null);

        schema.DeclaredAccessGroups.Should().BeNull();
    }
}
