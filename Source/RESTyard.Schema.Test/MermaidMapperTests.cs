using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using AwesomeAssertions;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Model;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace RESTyard.Schema.Test;

public class MermaidMapperTests() : VerifyBase()
{
    [Fact]
    public Task ToApiMap_MultipleEntities()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        var result = schema.ToApiMap();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_WithPropertiesAndActions()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        var result = schema.ToClassDiagram();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToApiMap_EmptySchema()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "EntryPoint",
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToApiMap();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_EmptySchema()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "EntryPoint",
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToClassDiagram();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToApiMap_SelfLinksExcluded()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "OnlyEntity",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "OnlyEntity",
                    Classes = new[] { "OnlyEntity" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "OnlyEntity",
                            TargetClasses = new[] { "OnlyEntity" },
                        },
                    },
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
            },
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToApiMap();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_NoProperties()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        var options = new MermaidMapperOptions { IncludeProperties = false };
        var result = schema.ToClassDiagram(options);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_NoActions()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        var options = new MermaidMapperOptions { IncludeActions = false };
        var result = schema.ToClassDiagram(options);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_NoPropertiesNoActions()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        var options = new MermaidMapperOptions { IncludeProperties = false, IncludeActions = false };
        var result = schema.ToClassDiagram(options);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_VariousPropertyTypes()
    {
        var propertiesSchema = JsonDocument.Parse("""
            {
                "type": "object",
                "properties": {
                    "id": { "type": "integer" },
                    "name": { "type": "string" },
                    "price": { "type": "number" },
                    "isActive": { "type": "boolean" },
                    "tags": { "type": "array", "items": { "type": "string" } },
                    "address": { "type": "object" },
                    "nullableField": { "type": "null" },
                    "noTypeField": { "description": "has no type keyword" },
                    "nestedObject": {
                        "type": "object",
                        "properties": {
                            "street": { "type": "string" },
                            "city": { "type": "string" }
                        }
                    },
                    "refField": { "$ref": "#/definitions/Brand" }
                }
            }
            """);

        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "Product",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Product",
                    Classes = new[] { "Product" },
                    PropertiesSchema = propertiesSchema,
                },
            },
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToClassDiagram();
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public void ToApiMap_annotates_restricted_entities()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            EntityTypes = new[]
            {
                new EntityTypeSchema { Name = "Root", Classes = new[] { "EntryPoint" } },
                new EntityTypeSchema { Name = "Admin", Classes = new[] { "Admin" }, AccessGroups = new[] { "admin" } },
            },
        };

        var result = schema.ToApiMap();

        result.Should().Contain("Root[\"Root\"]");
        result.Should().Contain("Admin[\"Admin [admin]\"]");
    }

    [Fact]
    public void ToClassDiagram_adds_note_for_restricted_entities()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            EntityTypes = new[]
            {
                new EntityTypeSchema { Name = "Root", Classes = new[] { "EntryPoint" } },
                new EntityTypeSchema { Name = "Admin", Classes = new[] { "Admin" }, AccessGroups = new[] { "admin", "sales" } },
            },
        };

        var result = schema.ToClassDiagram();

        result.Should().Contain("access: admin, sales");
    }

    [Fact]
    public void ToClassDiagram_access_appears_before_actions()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0.0",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Admin",
                    Classes = new[] { "Admin" },
                    AccessGroups = new[] { "admin" },
                    Actions = new[] { new ActionDescription { Name = "Delete" } },
                },
            },
        };

        var result = schema.ToClassDiagram();
        var accessIndex = result.IndexOf("access:", StringComparison.Ordinal);
        var deleteIndex = result.IndexOf("+Delete()", StringComparison.Ordinal);

        accessIndex.Should().BeGreaterThan(-1);
        deleteIndex.Should().BeGreaterThan(accessIndex);
    }
}
