using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using RESTyard.Schema.Markdown;
using RESTyard.Schema.Model;
using VerifyXunit;
using Xunit;

namespace RESTyard.Schema.Test;

public class MarkdownMapperTests() : VerifyBase()
{
    [Fact]
    public Task ToDocumentation_MultipleEntities()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        schema.Title = "Car Shop API";
        schema.Description = "A sample API for managing cars and customers.";
        schema.ApiVersion = "2.0.0";
        schema.ExternalDocsUrl = "https://example.com/docs";
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_EmptySchema()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            Title = "Empty API",
            EntryPointName = "EntryPoint",
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_NoToc()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        schema.Title = "Car Shop API";
        var options = new MarkdownMapperOptions { IncludeTableOfContents = false };
        var result = schema.ToDocumentation(options);
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_NoDiagram()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        schema.Title = "Car Shop API";
        var options = new MarkdownMapperOptions { IncludeDiagram = false };
        var result = schema.ToDocumentation(options);
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_NoTocNoDiagram()
    {
        var schema = TestSchemaFactory.CreateMultiEntitySchema();
        schema.Title = "Car Shop API";
        var options = new MarkdownMapperOptions { IncludeTableOfContents = false, IncludeDiagram = false };
        var result = schema.ToDocumentation(options);
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_WithDeprecation()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "EntryPoint",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "EntryPoint",
                    Classes = new[] { "EntryPoint" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "oldResource" },
                            TargetName = "OldEntity",
                            TargetClasses = new[] { "OldEntity" },
                            IsDeprecated = true,
                            Description = "Use newResource instead",
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "newResource" },
                            TargetName = "NewEntity",
                            TargetClasses = new[] { "NewEntity" },
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "OldAction",
                            IsDeprecated = true,
                            Description = "Use NewAction instead",
                        },
                    },
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "legacyItems" },
                            TargetName = "OldEntity",
                            TargetClasses = new[] { "OldEntity" },
                            IsDeprecated = true,
                            Description = "Use newResource link instead",
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "OldEntity",
                    Classes = new[] { "OldEntity" },
                    IsDeprecated = true,
                    DeprecationMessage = "This entity is deprecated. Use NewEntity instead.",
                },
                new EntityTypeSchema
                {
                    Name = "NewEntity",
                    Classes = new[] { "NewEntity" },
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_CyclicGraph()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "A",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "A",
                    Classes = new[] { "A" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "next" },
                            TargetName = "B",
                            TargetClasses = new[] { "B" },
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "B",
                    Classes = new[] { "B" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "next" },
                            TargetName = "C",
                            TargetClasses = new[] { "C" },
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "C",
                    Classes = new[] { "C" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "back" },
                            TargetName = "A",
                            TargetClasses = new[] { "A" },
                        },
                    },
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_OptionalElements()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "Root",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Root",
                    Classes = new[] { "Root" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "Root",
                            TargetClasses = new[] { "Root" },
                            IsMandatory = true,
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "optionalLink" },
                            TargetName = "Target",
                            TargetClasses = new[] { "Target" },
                            IsMandatory = false,
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "mandatoryLink" },
                            TargetName = "Target",
                            TargetClasses = new[] { "Target" },
                            IsMandatory = true,
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "OptionalAction",
                            IsMandatory = false,
                        },
                        new ActionDescription
                        {
                            Name = "MandatoryAction",
                            IsMandatory = true,
                        },
                    },
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "optionalEmbed" },
                            TargetName = "Target",
                            TargetClasses = new[] { "Target" },
                            IsMandatory = false,
                        },
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "mandatoryEmbed" },
                            TargetName = "Target",
                            TargetClasses = new[] { "Target" },
                            IsMandatory = true,
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Target",
                    Classes = new[] { "Target" },
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_ActionWithResult()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "Root",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Root",
                    Classes = new[] { "Root" },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "CreateItem",
                            Description = "Creates a new item.",
                            ResultName = "Item",
                            ResultClasses = new[] { "Item" },
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Item",
                    Classes = new[] { "Item" },
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_ActionWithParameters()
    {
        var paramSchema = JsonDocument.Parse(
            """{"type":"object","required":["carId"],"properties":{"carId":{"type":"integer","description":"The car identifier"},"color":{"type":"string"}}}"""
        ).RootElement.Clone();

        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "Root",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Root",
                    Classes = new[] { "Root" },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "BuyCar",
                            Description = "Purchase a car.",
                            ParameterSchema = paramSchema,
                        },
                    },
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }
}
