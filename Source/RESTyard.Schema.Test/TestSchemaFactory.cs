using System;
using System.Collections.Generic;
using System.Text.Json;
using RESTyard.Schema.Model;

namespace RESTyard.Schema.Test;

internal static class TestSchemaFactory
{
    internal static HypermediaApiSchema CreateMultiEntitySchema()
    {
        var customerProperties = JsonDocument.Parse(
            """{"type":"object","properties":{"name":{"type":"string"},"age":{"type":"integer"}}}"""
        );

        var buyCarParams = JsonDocument.Parse(
            """{"type":"object","properties":{"carId":{"type":"string"}}}"""
        );

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
            Definitions = new Dictionary<string, JsonDocument>(),
        };
    }
}
