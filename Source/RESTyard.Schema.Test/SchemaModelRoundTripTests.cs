using System.Collections.Generic;
using System.Text.Json;
using AwesomeAssertions;
using RESTyard.Schema.Model;
using Xunit;

namespace RESTyard.Schema.Test;

public class SchemaModelRoundTripTests
{
    private static HypermediaApiSchema CreateFullSchema()
    {
        var propertiesSchema = JsonDocument.Parse("""{"type":"object","properties":{"id":{"type":"integer"},"name":{"type":"string"}}}""");
        var parameterSchema = JsonDocument.Parse("""{"type":"object","properties":{"brand":{"type":"string"}},"required":["brand"]}""");
        var definitionSchema = JsonDocument.Parse("""{"type":"string","minLength":1}""");

        return new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            ApiVersion = "2.1.0",
            Title = "Car API",
            Description = "A demo car API",
            ExternalDocsUrl = "https://example.com/docs",
            EntryPointName = "EntryPoint",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Car",
                    Classes = new[] { "Car" },
                    Title = "A car entity",
                    Description = "Represents a single car",
                    PropertiesSchema = propertiesSchema,
                    Links = new[]
                    {
                        new LinkDescription
                        {
                            Relations = new[] { "self" },
                            TargetName = "Car",
                            TargetClasses = new[] { "Car" },
                            MediaTypes = new[] { "application/vnd.siren+json" },
                            Title = "Self link",
                            Description = "Link to itself",
                            IsMandatory = true,
                            IsDeprecated = false,
                            DeprecationMessage = null,
                        },
                    },
                    Actions = new[]
                    {
                        new ActionDescription
                        {
                            Name = "UpdateCar",
                            Title = "Update a car",
                            Description = "Updates the car properties",
                            ContentType = "application/json",
                            ParameterSchema = parameterSchema,
                            ResultName = "Car",
                            ResultClasses = new[] { "Car" },
                            IsMandatory = false,
                            IsFileUpload = false,
                            IsDeprecated = true,
                            DeprecationMessage = "Use PatchCar instead",
                        },
                    },
                    EmbeddedEntities = new[]
                    {
                        new EmbeddedEntityDescription
                        {
                            Relations = new[] { "item" },
                            TargetName = "Wheel",
                            TargetClasses = new[] { "Wheel" },
                            Title = "Wheels",
                            Description = "The car's wheels",
                            IsCollection = true,
                            IsMandatory = true,
                            IsDeprecated = false,
                            DeprecationMessage = null,
                        },
                    },
                    IsDeprecated = false,
                    DeprecationMessage = null,
                },
            },
            Definitions = new Dictionary<string, JsonDocument>
            {
                ["Brand"] = definitionSchema,
            },
        };
    }

    [Fact]
    public void RoundTrip_AllFields_ArePreserved()
    {
        var original = CreateFullSchema();

        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<HypermediaApiSchema>(json);

        deserialized.Should().NotBeNull();
        deserialized!.SchemaVersion.Should().Be("1.0");
        deserialized.ApiVersion.Should().Be("2.1.0");
        deserialized.Title.Should().Be("Car API");
        deserialized.Description.Should().Be("A demo car API");
        deserialized.ExternalDocsUrl.Should().Be("https://example.com/docs");
        deserialized.EntryPointName.Should().Be("EntryPoint");

        deserialized.EntityTypes.Should().HaveCount(1);
        var entity = deserialized.EntityTypes[0];
        entity.Name.Should().Be("Car");
        entity.Classes.Should().BeEquivalentTo(new[] { "Car" });
        entity.Title.Should().Be("A car entity");
        entity.Description.Should().Be("Represents a single car");
        entity.PropertiesSchema.Should().NotBeNull();
        entity.PropertiesSchema!.RootElement.GetProperty("type").GetString().Should().Be("object");
        entity.IsDeprecated.Should().BeFalse();
        entity.DeprecationMessage.Should().BeNull();

        entity.Links.Should().HaveCount(1);
        var link = entity.Links[0];
        link.Relations.Should().BeEquivalentTo(new[] { "self" });
        link.TargetName.Should().Be("Car");
        link.TargetClasses.Should().BeEquivalentTo(new[] { "Car" });
        link.MediaTypes.Should().BeEquivalentTo(new[] { "application/vnd.siren+json" });
        link.Title.Should().Be("Self link");
        link.IsMandatory.Should().BeTrue();
        link.IsDeprecated.Should().BeFalse();

        entity.Actions.Should().HaveCount(1);
        var action = entity.Actions[0];
        action.Name.Should().Be("UpdateCar");
        action.Title.Should().Be("Update a car");
        action.ContentType.Should().Be("application/json");
        action.ParameterSchema.Should().NotBeNull();
        var requiredArray = action.ParameterSchema!.RootElement.GetProperty("required");
        requiredArray.GetArrayLength().Should().BeGreaterThan(0);
        requiredArray[0].GetString().Should().Be("brand");
        action.ResultName.Should().Be("Car");
        action.ResultClasses.Should().BeEquivalentTo(new[] { "Car" });
        action.IsMandatory.Should().BeFalse();
        action.IsFileUpload.Should().BeFalse();
        action.IsDeprecated.Should().BeTrue();
        action.DeprecationMessage.Should().Be("Use PatchCar instead");

        entity.EmbeddedEntities.Should().HaveCount(1);
        var embedded = entity.EmbeddedEntities[0];
        embedded.Relations.Should().BeEquivalentTo(new[] { "item" });
        embedded.TargetName.Should().Be("Wheel");
        embedded.TargetClasses.Should().BeEquivalentTo(new[] { "Wheel" });
        embedded.Title.Should().Be("Wheels");
        embedded.IsCollection.Should().BeTrue();
        embedded.IsMandatory.Should().BeTrue();
        embedded.IsDeprecated.Should().BeFalse();

        deserialized.Definitions.Should().ContainKey("Brand");
        deserialized.Definitions["Brand"].RootElement.GetProperty("type").GetString().Should().Be("string");
    }

    [Fact]
    public void ExternalLink_WithoutTargetName_RoundTripsAndOmitsTargetNameInJson()
    {
        var link = new LinkDescription
        {
            Relations = new[] { "invoice-pdf" },
            TargetName = null,
            IsExternal = true,
            MediaTypes = new[] { "application/pdf", "text/html" },
            IsMandatory = true,
        };

        var json = JsonSerializer.Serialize(link);
        json.Should().NotContain("targetName");
        json.Should().Contain("\"isExternal\":true");
        json.Should().Contain("\"mediaTypes\":[\"application/pdf\",\"text/html\"]");

        var deserialized = JsonSerializer.Deserialize<LinkDescription>(json);
        deserialized.Should().NotBeNull();
        deserialized!.TargetName.Should().BeNull();
        deserialized.IsExternal.Should().BeTrue();
        deserialized.MediaTypes.Should().BeEquivalentTo(new[] { "application/pdf", "text/html" });
        deserialized.Relations.Should().BeEquivalentTo(new[] { "invoice-pdf" });
    }

    [Fact]
    public void InternalLink_OmitsIsExternalInJson()
    {
        var link = new LinkDescription
        {
            Relations = new[] { "self" },
            TargetName = "Car",
        };

        JsonSerializer.Serialize(link).Should().NotContain("isExternal");
    }

    [Fact]
    public void Serialization_UsesCamelCasePropertyNames()
    {
        var schema = CreateFullSchema();
        var json = JsonSerializer.Serialize(schema);

        json.Should().Contain("\"schemaVersion\"");
        json.Should().Contain("\"apiVersion\"");
        json.Should().Contain("\"externalDocsUrl\"");
        json.Should().Contain("\"entryPointName\"");
        json.Should().Contain("\"entityTypes\"");
        json.Should().Contain("\"propertiesSchema\"");
        json.Should().Contain("\"embeddedEntities\"");
        json.Should().Contain("\"targetName\"");
        json.Should().Contain("\"targetClasses\"");
        json.Should().Contain("\"mediaTypes\"");
        json.Should().Contain("\"isMandatory\"");
        json.Should().Contain("\"isDeprecated\"");
        json.Should().Contain("\"deprecationMessage\"");
        json.Should().Contain("\"contentType\"");
        json.Should().Contain("\"parameterSchema\"");
        json.Should().Contain("\"resultName\"");
        json.Should().Contain("\"resultClasses\"");
        json.Should().Contain("\"isFileUpload\"");
        json.Should().Contain("\"isCollection\"");

        // Should NOT contain PascalCase versions
        json.Should().NotContain("\"SchemaVersion\"");
        json.Should().NotContain("\"EntryPointName\"");
        json.Should().NotContain("\"EntityTypes\"");
    }

    [Fact]
    public void Serialization_OmitsNullProperties()
    {
        var schema = new HypermediaApiSchema
        {
            SchemaVersion = "1.0",
            ApiVersion = null,
            Title = null,
            Description = null,
            ExternalDocsUrl = null,
            EntryPointName = "EntryPoint",
            EntityTypes = new[]
            {
                new EntityTypeSchema
                {
                    Name = "Minimal",
                    Classes = new[] { "Minimal" },
                    Title = null,
                    Description = null,
                    PropertiesSchema = null,
                    Links = System.Array.Empty<LinkDescription>(),
                    Actions = System.Array.Empty<ActionDescription>(),
                    EmbeddedEntities = System.Array.Empty<EmbeddedEntityDescription>(),
                    IsDeprecated = false,
                    DeprecationMessage = null,
                },
            },
            Definitions = new Dictionary<string, JsonDocument>(),
        };

        var json = JsonSerializer.Serialize(schema);

        json.Should().NotContain("\"apiVersion\"");
        json.Should().NotContain("\"title\"");
        json.Should().NotContain("\"description\"");
        json.Should().NotContain("\"externalDocsUrl\"");
        json.Should().NotContain("\"propertiesSchema\"");
        json.Should().NotContain("\"deprecationMessage\"");

        // Required fields should still be present
        json.Should().Contain("\"schemaVersion\"");
        json.Should().Contain("\"entryPointName\"");
        json.Should().Contain("\"entityTypes\"");
    }
}
