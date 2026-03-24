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

    private class ParentWithEnum
    {
        public Status Status { get; set; }
    }
}
