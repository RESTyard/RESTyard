using System;
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

    // --- Dangling reference validation (GEN-14) ---

    private static List<EntityTypeSchema> EntityTypesWithDanglingReferences() =>
    [
        new()
        {
            Name = "Customer",
            Classes = ["Customer"],
            Links =
            [
                new LinkDescription { Relations = ["self"], TargetName = "Customer" },
                new LinkDescription { Relations = ["orders"], TargetName = "Order" },
            ],
            EmbeddedEntities = [new EmbeddedEntityDescription { Relations = ["address"], TargetName = "Address" }],
            Actions = [new ActionDescription { Name = "CreateNote", ResultName = "Note" }],
        },
    ];

    [Fact]
    public void ComposeSchema_logs_warning_per_dangling_reference()
    {
        var logger = new CapturingLogger();

        HypermediaSchemaBuilder.ComposeSchema(EntityTypesWithDanglingReferences(), null, logger);

        logger.Warnings.Should().HaveCount(3);
        logger.Warnings.Should().Contain(w => w.Contains("'Order'") && w.Contains("Customer link [orders]"));
        logger.Warnings.Should().Contain(w => w.Contains("'Address'") && w.Contains("Customer embedded [address]"));
        logger.Warnings.Should().Contain(w => w.Contains("'Note'") && w.Contains("Customer.CreateNote result"));
    }

    [Fact]
    public void ComposeSchema_dangling_references_add_no_placeholders_by_default()
    {
        var schema = HypermediaSchemaBuilder.ComposeSchema(EntityTypesWithDanglingReferences(), null, null);

        schema.EntityTypes.Should().ContainSingle().Which.Name.Should().Be("Customer");
    }

    [Fact]
    public void ComposeSchema_AllowUnresolvedReferences_adds_empty_placeholder_entity_types()
    {
        var options = new HypermediaSchemaOptions { AllowUnresolvedReferences = true };

        var schema = HypermediaSchemaBuilder.ComposeSchema(EntityTypesWithDanglingReferences(), options, null);

        schema.EntityTypes.Should().HaveCount(4);
        var placeholder = schema.EntityTypes.Should().ContainSingle(e => e.Name == "Order").Which;
        placeholder.PropertiesSchema.Should().BeNull();
        placeholder.Links.Should().BeEmpty();
        placeholder.Actions.Should().BeEmpty();
        placeholder.EmbeddedEntities.Should().BeEmpty();
        schema.EntityTypes.Should().Contain(e => e.Name == "Address");
        schema.EntityTypes.Should().Contain(e => e.Name == "Note");
    }

    [Fact]
    public void ComposeSchema_placeholder_added_once_for_multiple_references_to_same_name()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new()
            {
                Name = "Customer",
                Links =
                [
                    new LinkDescription { Relations = ["orders"], TargetName = "Order" },
                    new LinkDescription { Relations = ["lastOrder"], TargetName = "Order" },
                ],
            },
        };
        var options = new HypermediaSchemaOptions { AllowUnresolvedReferences = true };
        var logger = new CapturingLogger();

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, [], options, logger);

        schema.EntityTypes.Should().HaveCount(2);
        var warning = logger.Warnings.Should().ContainSingle().Which;
        warning.Should().Contain("Customer link [orders]").And.Contain("Customer link [lastOrder]");
    }

    [Fact]
    public void ComposeSchema_resolved_and_external_references_do_not_warn()
    {
        var entityTypes = new List<EntityTypeSchema>
        {
            new()
            {
                Name = "Customer",
                Links =
                [
                    new LinkDescription { Relations = ["self"], TargetName = "Customer" },
                    new LinkDescription { Relations = ["docs"], IsExternal = true },
                ],
                Actions = [new ActionDescription { Name = "Rename" }],
            },
        };
        var logger = new CapturingLogger();

        var schema = HypermediaSchemaBuilder.ComposeSchema(entityTypes, null, logger);

        logger.Warnings.Should().BeEmpty();
        schema.EntityTypes.Should().ContainSingle();
    }

    private sealed class CapturingLogger : ILogger
    {
        public List<string> Warnings { get; } = new();

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state,
            Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (logLevel == LogLevel.Warning)
            {
                Warnings.Add(formatter(state, exception));
            }
        }
    }
}
