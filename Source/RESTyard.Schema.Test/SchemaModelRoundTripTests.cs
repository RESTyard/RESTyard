using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using Json.Schema;
using RESTyard.Schema.Model;
using Xunit;

namespace RESTyard.Schema.Test;

public class SchemaModelRoundTripTests
{
    private static HypermediaApiSchema CreateFullSchema()
    {
        var propertiesSchema = JsonSchema.FromText("""{"type":"object","properties":{"id":{"type":"integer"},"name":{"type":"string"}}}""");
        var parameterSchema = JsonSchema.FromText("""{"type":"object","properties":{"brand":{"type":"string"}},"required":["brand"]}""");
        var definitionSchema = JsonSchema.FromText("""{"type":"string","minLength":1}""");

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
                            MediaType = "application/vnd.siren+json",
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
            Definitions = new Dictionary<string, JsonSchema>
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
        var propsType = entity.PropertiesSchema!.Keywords!.OfType<TypeKeyword>().FirstOrDefault()?.Type;
        propsType.Should().Be(SchemaValueType.Object);
        entity.IsDeprecated.Should().BeFalse();
        entity.DeprecationMessage.Should().BeNull();

        entity.Links.Should().HaveCount(1);
        var link = entity.Links[0];
        link.Relations.Should().BeEquivalentTo(new[] { "self" });
        link.TargetName.Should().Be("Car");
        link.TargetClasses.Should().BeEquivalentTo(new[] { "Car" });
        link.MediaType.Should().Be("application/vnd.siren+json");
        link.Title.Should().Be("Self link");
        link.IsMandatory.Should().BeTrue();
        link.IsDeprecated.Should().BeFalse();

        entity.Actions.Should().HaveCount(1);
        var action = entity.Actions[0];
        action.Name.Should().Be("UpdateCar");
        action.Title.Should().Be("Update a car");
        action.ContentType.Should().Be("application/json");
        action.ParameterSchema.Should().NotBeNull();
        var requiredProps = action.ParameterSchema!.Keywords!.OfType<RequiredKeyword>().FirstOrDefault()?.Properties;
        requiredProps.Should().NotBeNull();
        requiredProps![0].Should().Be("brand");
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
        var brandType = deserialized.Definitions["Brand"].Keywords!.OfType<TypeKeyword>().FirstOrDefault()?.Type;
        brandType.Should().Be(SchemaValueType.String);
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
        json.Should().Contain("\"mediaType\"");
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
            Definitions = new Dictionary<string, JsonSchema>(),
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
