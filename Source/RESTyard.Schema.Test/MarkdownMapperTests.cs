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
        var schema = CreateRichSchema();
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    private static HypermediaApiSchema CreateRichSchema()
    {
        var customerProperties = JsonDocument.Parse("""
            {
                "type": "object",
                "required": ["name", "email"],
                "properties": {
                    "name": { "type": "string", "description": "Full name of the customer" },
                    "email": { "type": "string", "description": "Primary email address", "format": "email" },
                    "age": { "type": "integer", "description": "Age in years" },
                    "isVip": { "type": "boolean", "description": "Whether the customer has VIP status" },
                    "address": { "$ref": "#/definitions/Address", "description": "Home address" }
                }
            }
            """);

        var carProperties = JsonDocument.Parse("""
            {
                "type": "object",
                "required": ["brand", "model", "price"],
                "properties": {
                    "brand": { "type": "string", "description": "Car manufacturer" },
                    "model": { "type": "string", "description": "Model name" },
                    "year": { "type": "integer", "description": "Year of manufacture" },
                    "price": { "type": "number", "description": "Price in EUR" },
                    "color": { "type": "string", "default": "white" },
                    "features": { "type": "array", "items": { "type": "string" }, "description": "List of optional features" }
                }
            }
            """);

        var buyCarParams = JsonDocument.Parse("""
            {
                "type": "object",
                "required": ["carId"],
                "properties": {
                    "carId": { "type": "integer", "description": "The car to purchase" },
                    "financingOption": { "type": "string", "description": "Payment plan", "enum": ["cash", "lease", "finance"], "default": "cash" }
                }
            }
            """);

        var createCustomerParams = JsonDocument.Parse("""
            {
                "type": "object",
                "required": ["name", "email"],
                "properties": {
                    "name": { "type": "string", "description": "Full name" },
                    "email": { "type": "string", "description": "Email address" },
                    "referralCode": { "type": "string", "description": "Optional referral code for discounts" }
                }
            }
            """);

        return new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            Title = "Car Shop API",
            Description = "A sample API for managing cars and customers.",
            ApiVersion = "2.0.0",
            ExternalDocsUrl = "https://example.com/docs",
            EntryPointName = "EntryPoint",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "EntryPoint",
                    Title = "API Entry Point",
                    Description = "The root resource of the Car Shop API. Start here to discover available resources.",
                    Classes = new[] { "EntryPoint" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "EntryPoint",
                            TargetClasses = new[] { "EntryPoint" },
                            IsMandatory = true,
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "customers" },
                            TargetName = "CustomersRoot",
                            TargetClasses = new[] { "CustomersRoot" },
                            IsMandatory = true,
                            Description = "Browse and manage customers",
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "cars" },
                            TargetName = "CarsRoot",
                            TargetClasses = new[] { "CarsRoot" },
                            IsMandatory = true,
                            Description = "Browse available cars",
                        },
                    },
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
                new EntityTypeSchema
                {
                    Name = "CustomersRoot",
                    Title = "Customers Collection",
                    Description = "Lists all customers with the ability to create new ones.",
                    Classes = new[] { "CustomersRoot", "Collection" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "CustomersRoot",
                            TargetClasses = new[] { "CustomersRoot" },
                            IsMandatory = true,
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "CreateCustomer",
                            Title = "Create a new customer",
                            Description = "Registers a new customer in the system.",
                            ParameterSchema = createCustomerParams,
                            ResultName = "Customer",
                            ResultClasses = new[] { "Customer" },
                            IsMandatory = true,
                        },
                    },
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "item" },
                            TargetName = "Customer",
                            TargetClasses = new[] { "Customer" },
                            IsCollection = true,
                            IsMandatory = true,
                            Description = "Customer entries in the collection",
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Customer",
                    Title = "Customer",
                    Description = "Represents an individual customer with their profile and available actions.",
                    Classes = new[] { "Customer" },
                    PropertiesSchema = customerProperties,
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "Customer",
                            TargetClasses = new[] { "Customer" },
                            IsMandatory = true,
                        },
                        new LinkDescription
                        {
                            Relations = new[] { "orders" },
                            TargetName = "CarsRoot",
                            TargetClasses = new[] { "CarsRoot" },
                            Description = "Cars purchased by this customer",
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "MarkAsFavorite",
                            Title = "Mark as favorite",
                            Description = "Marks this customer as a favorite for quick access.",
                        },
                        new ActionDescription
                        {
                            Name = "BuyCar",
                            Title = "Purchase a car",
                            Description = "Initiates a car purchase for this customer.",
                            ParameterSchema = buyCarParams,
                            ResultName = "Car",
                            ResultClasses = new[] { "Car" },
                        },
                    },
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
                new EntityTypeSchema
                {
                    Name = "CarsRoot",
                    Title = "Cars Collection",
                    Description = "Browseable list of all available cars.",
                    Classes = new[] { "CarsRoot", "Collection" },
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "CarsRoot",
                            TargetClasses = new[] { "CarsRoot" },
                            IsMandatory = true,
                        },
                    },
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "item" },
                            TargetName = "Car",
                            TargetClasses = new[] { "Car" },
                            IsCollection = true,
                            IsMandatory = true,
                            Description = "Car entries in the collection",
                        },
                    },
                },
                new EntityTypeSchema
                {
                    Name = "Car",
                    Title = "Car",
                    Description = "Represents an individual car available for purchase.",
                    Classes = new[] { "Car" },
                    PropertiesSchema = carProperties,
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "Car",
                            TargetClasses = new[] { "Car" },
                            IsMandatory = true,
                        },
                    },
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                },
            },
            Definitions = new Dictionary<string, JsonDocument>
            {
                ["Address"] = JsonDocument.Parse("""
                    {
                        "type": "object",
                        "description": "A postal address.",
                        "required": ["street", "city"],
                        "properties": {
                            "street": { "type": "string", "description": "Street name and number" },
                            "city": { "type": "string", "description": "City name" },
                            "zip": { "type": "string", "description": "Postal code" }
                        }
                    }
                    """),
            },
        };
    }

    [Fact]
    public Task ToDocumentation_EmptySchema()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            Title = "Empty API",
            EntryPointName = "EntryPoint",
            Definitions = new Dictionary<string, JsonDocument>(),
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
            Definitions = new Dictionary<string, JsonDocument>(),
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
            Definitions = new Dictionary<string, JsonDocument>(),
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
            Definitions = new Dictionary<string, JsonDocument>(),
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
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }

    [Fact]
    public Task ToDocumentation_ActionWithParameters()
    {
        var paramSchema = JsonDocument.Parse(
            """{"type":"object","required":["carId"],"properties":{"carId":{"type":"integer","description":"The car identifier"},"color":{"type":"string"}}}"""
        );

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
            Definitions = new Dictionary<string, JsonDocument>(),
        };
        var result = schema.ToDocumentation();
        return Verify(result, extension: "md");
    }
}
