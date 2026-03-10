using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text.Json;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Model;
using VerifyTests;
using VerifyXunit;
using Xunit;

namespace RESTyard.Schema.Test;

public class MermaidMapperTests() : VerifyBase()
{
    private static HypermediaApiSchema CreateMultiEntitySchema()
    {
        var customerProperties = JsonDocument.Parse(
            """{"type":"object","properties":{"name":{"type":"string"},"age":{"type":"integer"}}}"""
        ).RootElement.Clone();

        var buyCarParams = JsonDocument.Parse(
            """{"type":"object","properties":{"carId":{"type":"string"}}}"""
        ).RootElement.Clone();

        return new HypermediaApiSchema
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
                            Relations = new[] { "self" },
                            TargetName = "EntryPoint",
                            TargetClasses = new[] { "EntryPoint" },
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "customers" },
                            TargetName = "CustomersRoot",
                            TargetClasses = new[] { "CustomersRoot" },
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "cars" },
                            TargetName = "CarsRoot",
                            TargetClasses = new[] { "CarsRoot" },
                        },
                    },
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
                new EntityTypeSchema
                {
                    Name = "CustomersRoot",
                    Classes = new[] { "CustomersRoot" },
                    Links = System.Array.Empty<LinkDescription>(),
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "item" },
                            TargetName = "Customer",
                            TargetClasses = new[] { "Customer" },
                            IsCollection = true,
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Customer",
                    Classes = new[] { "Customer" },
                    PropertiesSchema = customerProperties,
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "Customer",
                            TargetClasses = new[] { "Customer" },
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription { Name = "MarkAsFavorite" },
                        new ActionDescription { Name = "BuyCar", ParameterSchema = buyCarParams },
                    },
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
                new EntityTypeSchema
                {
                    Name = "CarsRoot",
                    Classes = new[] { "CarsRoot" },
                    Links = System.Array.Empty<LinkDescription>(),
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "item" },
                            TargetName = "Car",
                            TargetClasses = new[] { "Car" },
                            IsCollection = true,
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Car",
                    Classes = new[] { "Car" },
                    Links = System.Array.Empty<LinkDescription>(),
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
            },
            Definitions = new Dictionary<string, JsonElement>(),
        };
    }

    [Fact]
    public Task ToEntityGraph_MultipleEntities()
    {
        var schema = CreateMultiEntitySchema();
        var result = MermaidMapper.ToEntityGraph(schema);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToClassDiagram_WithPropertiesAndActions()
    {
        var schema = CreateMultiEntitySchema();
        var result = MermaidMapper.ToClassDiagram(schema);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToEntityGraph_EmptySchema()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            EntryPointName = "EntryPoint",
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = MermaidMapper.ToEntityGraph(schema);
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
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = MermaidMapper.ToClassDiagram(schema);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }

    [Fact]
    public Task ToEntityGraph_SelfLinksExcluded()
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
            Definitions = new Dictionary<string, JsonElement>(),
        };
        var result = MermaidMapper.ToEntityGraph(schema);
        var markdown = $"```mermaid\n{result}\n```";
        return Verify(markdown, extension: "md");
    }
}
