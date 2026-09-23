using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using RESTyard.Schema.Mermaid;
using RESTyard.Schema.Markdown;
using RESTyard.Schema.Model;
using RESTyard.Schema.SchemaGeneration;
using Xunit;

namespace RESTyard.Schema.Test.SchemaGeneration;

public class ComplexTypeDefinitionRefinerTests
{
    private readonly JsonSchemaFactory _factory = new(extractComplexTypesToDefs: true);
    private readonly JsonSchemaFactory _factoryWithoutRefiner = new(extractComplexTypesToDefs: false);

    [Fact]
    public void Nested_object_is_extracted_to_defs()
    {
        var schema = _factory.Generate(typeof(ParentWithNestedObject));
        var root = schema.RootElement;

        // Should have $defs
        root.TryGetProperty("$defs", out var defs).Should().BeTrue();

        // HomeAddress property should be a $ref
        var props = root.GetProperty("properties");
        var homeAddress = props.GetProperty("HomeAddress");
        homeAddress.TryGetProperty("$ref", out _).Should().BeTrue();
    }

    [Fact]
    public void Nested_object_without_refiner_is_inlined()
    {
        var schema = _factoryWithoutRefiner.Generate(typeof(ParentWithNestedObject));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");
        var homeAddress = props.GetProperty("HomeAddress");

        // Without refiner, should be inlined as type: object
        homeAddress.GetProperty("type").GetString().Should().Be("object");
        homeAddress.TryGetProperty("$ref", out _).Should().BeFalse();
    }

    [Fact]
    public void Primitive_properties_are_not_extracted()
    {
        var schema = _factory.Generate(typeof(ParentWithNestedObject));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");

        // Name should be inline string, not a $ref
        var name = props.GetProperty("Name");
        name.TryGetProperty("$ref", out _).Should().BeFalse();
        name.GetProperty("type").GetString().Should().Be("string");

        // Age should be inline integer
        var age = props.GetProperty("Age");
        age.TryGetProperty("$ref", out _).Should().BeFalse();
    }

    [Fact]
    public void List_of_complex_type_extracts_element_type()
    {
        var schema = _factory.Generate(typeof(ParentWithCollection));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");
        var addresses = props.GetProperty("Addresses");

        // Should be array
        addresses.GetProperty("type").GetString().Should().Be("array");

        // Items should be a $ref to the Address type
        var items = addresses.GetProperty("items");
        items.TryGetProperty("$ref", out _).Should().BeTrue();
    }

    [Fact]
    public void Enum_properties_are_not_extracted()
    {
        var schema = _factory.Generate(typeof(ParentWithEnum));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");
        var status = props.GetProperty("Status");

        // Enum should not be a $ref
        status.TryGetProperty("$ref", out _).Should().BeFalse();
    }

    [Fact]
    public void Generated_schema_compiles_and_is_valid_json()
    {
        var schema = _factory.Generate(typeof(ParentWithNestedObject));
        var json = schema.RootElement.GetRawText();

        // Should be valid JSON
        var parsed = JsonDocument.Parse(json);
        parsed.Should().NotBeNull();
    }

    [Fact]
    public void Mermaid_class_diagram_shows_type_name_instead_of_object()
    {
        var schema = BuildSchemaWithNestedObject();
        var mermaid = schema.ToClassDiagram();

        // Should show the type name from the $ref, not "object"
        mermaid.Should().NotContain("object HomeAddress");
        // Should contain the resolved type name
        mermaid.Should().Contain("HomeAddress");
    }

    [Fact]
    public void Markdown_documentation_shows_type_name_instead_of_object()
    {
        var schema = BuildSchemaWithNestedObject();
        var markdown = schema.ToDocumentation();

        // Should not show "object" for the Address property
        markdown.Should().NotContain("| HomeAddress | object |");
    }

    [Fact]
    public void Definitions_catalog_is_populated_from_entity_defs()
    {
        var schema = BuildSchemaWithNestedObject();

        schema.Definitions.Should().NotBeEmpty();
        // Address type should appear in Definitions
        schema.Definitions.Keys.Should().Contain(k => k.Contains("address", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Definitions_catalog_deduplicates_shared_types()
    {
        var customerSchema = _factory.Generate(typeof(ParentWithNestedObject));
        var orderSchema = _factory.Generate(typeof(AnotherParentWithAddress));

        var apiSchema = HypermediaSchemaBuilder.ComposeSchema(
            new List<EntityTypeSchema>
            {
                new() { Name = "Customer", Classes = ["Customer"], PropertiesSchema = customerSchema },
                new() { Name = "Order", Classes = ["Order"], PropertiesSchema = orderSchema },
            },
            null, null);

        // Address appears in both entities but should be deduplicated in Definitions
        var addressDefs = apiSchema.Definitions
            .Where(d => d.Key.Contains("address", StringComparison.OrdinalIgnoreCase))
            .ToList();
        addressDefs.Should().HaveCount(1);
    }

    [Fact]
    public void Nullable_complex_property_references_definition_with_null_alternative()
    {
        var root = _factory.Generate(typeof(ParentWithNullableObject)).RootElement;

        var oneOf = root.GetProperty("properties").GetProperty("OtherAddress").GetProperty("oneOf");
        oneOf.GetArrayLength().Should().Be(2);
        oneOf[0].GetProperty("$ref").GetString().Should().StartWith("#/$defs/");
        oneOf[1].GetProperty("type").GetString().Should().Be("null");
    }

    [Fact]
    public void Type_used_only_as_nullable_is_moved_to_defs()
    {
        var root = _factory.Generate(typeof(ParentWithOnlyNullableObject)).RootElement;

        var reference = root.GetProperty("properties").GetProperty("MaybeAddress")
            .GetProperty("oneOf")[0].GetProperty("$ref").GetString()!;
        var definition = root.GetProperty("$defs").GetProperty(reference.Substring("#/$defs/".Length));
        definition.GetProperty("type").GetString().Should().Be("object");
        definition.GetProperty("properties").TryGetProperty("Street", out _).Should().BeTrue();
    }

    [Fact]
    public void Each_type_id_occurs_once()
    {
        var root = _factory.Generate(typeof(ParentWithNullableObject)).RootElement;

        var ids = CollectIds(root).ToList();
        ids.Should().OnlyHaveUniqueItems();
        ids.Should().Contain(id => id.EndsWith("+Address"));
    }

    [Fact]
    public void Member_annotation_is_kept_next_to_nullable_reference()
    {
        var root = _factory.Generate(typeof(ParentWithNullableObject)).RootElement;

        var otherAddress = root.GetProperty("properties").GetProperty("OtherAddress");
        otherAddress.GetProperty("description").GetString().Should().Be("Used when shipping elsewhere");
        otherAddress.TryGetProperty("properties", out _).Should().BeFalse();
    }

    [Fact]
    public void Nullable_complex_property_without_refiner_is_inlined()
    {
        var root = _factoryWithoutRefiner.Generate(typeof(ParentWithNullableObject)).RootElement;

        var otherAddress = root.GetProperty("properties").GetProperty("OtherAddress");
        otherAddress.TryGetProperty("oneOf", out _).Should().BeFalse();
        otherAddress.TryGetProperty("properties", out _).Should().BeTrue();
    }

    [Fact]
    public void Mermaid_and_Markdown_show_type_name_for_nullable_complex_property()
    {
        var schema = HypermediaSchemaBuilder.ComposeSchema(
            new List<EntityTypeSchema>
            {
                new()
                {
                    Name = "Parent",
                    Classes = ["Parent"],
                    PropertiesSchema = _factory.Generate(typeof(ParentWithNullableObject)),
                },
            },
            new HypermediaSchemaOptions { EntryPointName = "Parent" },
            null);

        // The nullable member renders the same type as the non-nullable one, not "object"
        var mermaid = schema.ToClassDiagram();
        var homeType = TypeBefore(mermaid, " HomeAddress");
        homeType.Should().NotBe("object");
        TypeBefore(mermaid, " OtherAddress").Should().Be(homeType);

        var markdown = schema.ToDocumentation();
        var homeColumn = TypeColumn(markdown, "| HomeAddress |");
        homeColumn.Should().StartWith("[");
        TypeColumn(markdown, "| OtherAddress |").Should().Be(homeColumn);

        // Both renderers resolve the display name from the definition's $id
        homeColumn.Should().StartWith($"[{homeType}]");
    }

    private static string TypeColumn(string markdown, string rowStart)
        => markdown.Split('\n').First(l => l.StartsWith(rowStart)).Split('|')[2].Trim();

    private static string TypeBefore(string text, string memberSuffix)
    {
        var line = text.Split('\n').Single(l => l.TrimEnd().EndsWith(memberSuffix));
        return line.Trim().TrimStart('+').Split(' ')[0];
    }

    private static IEnumerable<string> CollectIds(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.Object:
                foreach (var property in element.EnumerateObject())
                {
                    if (property.Name == "$id")
                        yield return property.Value.GetString()!;
                    foreach (var id in CollectIds(property.Value))
                        yield return id;
                }
                break;
            case JsonValueKind.Array:
                foreach (var item in element.EnumerateArray())
                foreach (var id in CollectIds(item))
                    yield return id;
                break;
        }
    }

    private HypermediaApiSchema BuildSchemaWithNestedObject()
    {
        var propertiesSchema = _factory.Generate(typeof(ParentWithNestedObject));
        return HypermediaSchemaBuilder.ComposeSchema(
            new List<EntityTypeSchema>
            {
                new()
                {
                    Name = "Parent",
                    Classes = ["Parent"],
                    PropertiesSchema = propertiesSchema,
                },
            },
            new HypermediaSchemaOptions { EntryPointName = "Parent" },
            null);
    }

    // --- Test types ---

    private class Address
    {
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
    }

    private class ParentWithNestedObject
    {
        public string Name { get; set; } = string.Empty;
        public int Age { get; set; }
        public Address HomeAddress { get; set; } = new();
    }

    private class ParentWithCollection
    {
        public List<Address> Addresses { get; set; } = new();
    }

    private enum Status { Active, Inactive }

    private class AnotherParentWithAddress
    {
        public string OrderNumber { get; set; } = string.Empty;
        public Address ShippingAddress { get; set; } = new();
    }

    private class ParentWithNullableObject
    {
        public Address HomeAddress { get; set; } = new();

        [Json.Schema.Generation.Description("Used when shipping elsewhere")]
        public Address? OtherAddress { get; set; }
    }

    private class ParentWithOnlyNullableObject
    {
        public Address? MaybeAddress { get; set; }
    }

    private class ParentWithEnum
    {
        public Status Status { get; set; }
    }
}
