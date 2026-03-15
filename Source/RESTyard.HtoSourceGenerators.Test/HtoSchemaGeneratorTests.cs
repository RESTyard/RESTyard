using System.Linq;
using AwesomeAssertions;
using Json.Schema;
using Microsoft.CodeAnalysis;
using Xunit;

namespace RESTyard.HtoSourceGenerators.Test;

public class HtoSchemaGeneratorTests
{
    [Fact]
    public void SimpleHto_generates_correct_schema()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);

        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle(t =>
            t.FilePath.Contains("HypermediaCustomerHtoSirenMapper"));

        var source = GetGeneratedSource(result, "HypermediaCustomerHto");
        source.Should().Contain("namespace TestHtos;");
        source.Should().Contain("Name = \"Customer\"");
        source.Should().Contain("Title = \"Customer\"");
        source.Should().Contain("Classes = new[] { \"Customer\" }");
    }

    [Fact]
    public void SimpleHto_generated_source_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.SimpleHto);
    }

    [Fact]
    public void FullHto_generates_mapper_per_type_and_compiles()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);

        result.Diagnostics.Should().BeEmpty();

        var fileNames = result.GeneratedTrees
            .Select(t => System.IO.Path.GetFileName(t.FilePath))
            .OrderBy(n => n)
            .ToArray();

        fileNames.Should().BeEquivalentTo(
            "HypermediaAddressHtoSirenMapper.g.cs",
            "HypermediaCustomerHtoSirenMapper.g.cs");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.FullHto);
    }

    [Fact]
    public void Hto_without_title_omits_title_in_schema()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Classes = ["Truck"])]
            public class HypermediaTruckHto : HypermediaObject
            {
                public string Brand { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var generated = GetGeneratedSource(result, "HypermediaTruckHto");

        generated.Should().Contain("Name = \"Truck\"");
        generated.Should().NotContain("Title =");
    }

    [Fact]
    public void Hto_with_empty_title_omits_title_in_schema()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "", Classes = ["Truck"])]
            public class HypermediaTruckHto : HypermediaObject
            {
                public string Brand { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var generated = GetGeneratedSource(result, "HypermediaTruckHto");

        generated.Should().NotContain("Title =");
    }

    [Fact]
    public void Hto_with_multiple_classes()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "Cars Root", Classes = ["CarsRoot", "CollectionRoot"])]
            public class HypermediaCarsRootHto : HypermediaObject
            {
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var generated = GetGeneratedSource(result, "HypermediaCarsRootHto");

        generated.Should().Contain("Name = \"CarsRoot\"");
        generated.Should().Contain("Classes = new[] { \"CarsRoot\", \"CollectionRoot\" }");
    }

    [Fact]
    public void Class_without_IHypermediaObject_is_ignored()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "Not an HTO", Classes = ["Fake"])]
            public class NotAnHto
            {
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void SimpleHto_calls_factory_for_each_property_type()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        // Method takes IJsonSchemaFactory parameter
        source.Should().Contain("GetSchema(IJsonSchemaFactory schemaFactory)");

        // Calls factory for each property type with correct property names
        source.Should().Contain("propertySchemas[\"Name\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"Age\"] = schemaFactory.Generate(typeof(");

        // Calls SchemaHelper to compose
        source.Should().Contain("SchemaHelper.BuildPropertiesSchema(propertySchemas)");
        source.Should().Contain("PropertiesSchema = propertiesSchema");
    }

    [Fact]
    public void Hto_with_various_property_types_emits_correct_typeof_calls_and_compiles()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithVariousPropertyTypes);
        var source = GetGeneratedSource(result, "HypermediaAllTypesHto");

        // FullyQualifiedFormat uses keyword aliases for built-in types
        source.Should().Contain("typeof(string)");
        source.Should().Contain("typeof(bool)");
        source.Should().Contain("typeof(int)");
        source.Should().Contain("typeof(long)");
        source.Should().Contain("typeof(double)");
        source.Should().Contain("typeof(decimal)");
        // Well-known types use global:: prefix
        source.Should().Contain("typeof(global::System.DateTime)");
        source.Should().Contain("typeof(global::System.DateTimeOffset)");
        source.Should().Contain("typeof(global::System.DateOnly)");
        source.Should().Contain("typeof(global::System.TimeOnly)");
        source.Should().Contain("typeof(global::System.TimeSpan)");
        source.Should().Contain("typeof(global::System.Uri)");
        source.Should().Contain("typeof(global::System.Guid)");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithVariousPropertyTypes);
    }

    [Fact]
    public void Nullable_properties_emit_nullable_typeof()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithNullableProperties);
        var source = GetGeneratedSource(result, "HypermediaNullableHto");

        // Nullable value types should emit their nullable typeof
        source.Should().Contain("propertySchemas[\"OptionalCount\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"OptionalFlag\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"OptionalDate\"] = schemaFactory.Generate(typeof(");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithNullableProperties);
    }

    [Fact]
    public void Enum_properties_emit_typeof_and_compile()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithEnumProperties);
        var source = GetGeneratedSource(result, "HypermediaWithEnumHto");

        source.Should().Contain("propertySchemas[\"CurrentStatus\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"CurrentPriority\"] = schemaFactory.Generate(typeof(");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithEnumProperties);
    }

    [Fact]
    public void Collection_and_array_properties_emit_typeof_and_compile()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithCollections);
        var source = GetGeneratedSource(result, "HypermediaWithCollectionsHto");

        source.Should().Contain("propertySchemas[\"Tags\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"Scores\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"Flags\"] = schemaFactory.Generate(typeof(");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithCollections);
    }

    [Fact]
    public void Nested_object_property_emits_typeof_and_compiles()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithNestedObject);
        var source = GetGeneratedSource(result, "HypermediaWithNestedHto");

        source.Should().Contain("propertySchemas[\"Name\"] = schemaFactory.Generate(typeof(");
        source.Should().Contain("propertySchemas[\"HomeAddress\"] = schemaFactory.Generate(typeof(");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithNestedObject);
    }

    [Fact]
    public void HypermediaProperty_Name_renames_and_FormatterIgnore_excludes()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithPropertyAttributes);
        var source = GetGeneratedSource(result, "HypermediaWithAttributesHto");

        // [HypermediaProperty(Name = "FullName")] should use custom name as the key
        source.Should().Contain("propertySchemas[\"FullName\"]");
        // Original C# property name "Name" should not appear as a schema key
        source.Should().NotContain("propertySchemas[\"Name\"]");

        // [FormatterIgnoreHypermediaProperty] should exclude the property entirely
        source.Should().NotContain("InternalId");

        // Regular property remains
        source.Should().Contain("propertySchemas[\"Age\"]");
    }

    [Fact]
    public void Links_and_actions_are_excluded_from_properties_schema()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        // Data properties should be present
        source.Should().Contain("propertySchemas[\"Name\"]");
        source.Should().Contain("propertySchemas[\"Age\"]");

        // Links (marked with [Relations]) should be excluded
        source.Should().NotContain("\"Self\"");
        source.Should().NotContain("\"BestFriend\"");

        // Actions (marked with [HypermediaAction]) should be excluded
        source.Should().NotContain("\"MarkAsFavorite\"");
        source.Should().NotContain("\"BuyCar\"");

        // Embedded entities (marked with [Relations]) should be excluded
        source.Should().NotContain("\"Address\"");
    }

    [Fact]
    public void Hto_without_properties_omits_PropertiesSchema_and_factory_parameter()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "Empty", Classes = ["Empty"])]
            public class HypermediaEmptyHto : HypermediaObject
            {
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var generated = GetGeneratedSource(result, "HypermediaEmptyHto");

        generated.Should().NotContain("PropertiesSchema");
        generated.Should().NotContain("IJsonSchemaFactory");
        generated.Should().Contain("GetSchema()");
    }

    [Theory]
    [InlineData("HypermediaCustomerHto", "Customer")]
    [InlineData("HypermediaCustomer", "Customer")]
    [InlineData("CustomerHto", "Customer")]
    [InlineData("Customer", "Customer")]
    [InlineData("HypermediaEntrypointHto", "Entrypoint")]
    [InlineData("HypermediaCarsRootHto", "CarsRoot")]
    public void DeriveSchemaName_produces_expected_name(string className, string expected)
    {
        HtoSchemaGenerator.DeriveSchemaName(className).Should().Be(expected);
    }

    [Fact]
    public void SimpleHto_GetSchema_returns_valid_PropertiesSchema()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.SimpleHto);

        schema.Name.Should().Be("Customer");
        schema.Title.Should().Be("Customer");
        schema.Classes.Should().BeEquivalentTo("Customer");
        schema.PropertiesSchema.Should().NotBeNull();

        var properties = schema.PropertiesSchema!.GetProperties();
        properties.Should().NotBeNull();
        properties.Should().ContainKey("Name");
        properties.Should().ContainKey("Age");

        properties!["Name"].GetJsonType().Should().Be(SchemaValueType.String);
        properties["Age"].GetJsonType().Should().Be(SchemaValueType.Integer);
    }

    [Fact]
    public void FullHto_GetSchema_excludes_links_actions_includes_data_properties()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.FullHto);

        schema.Name.Should().Be("Customer");
        schema.PropertiesSchema.Should().NotBeNull();

        var properties = schema.PropertiesSchema!.GetProperties();
        properties.Should().NotBeNull();

        // Data properties should be present
        properties.Should().ContainKey("Name");
        properties.Should().ContainKey("Age");

        // Links, actions, and embedded entities should be excluded
        properties.Should().NotContainKey("Self");
        properties.Should().NotContainKey("BestFriend");
        properties.Should().NotContainKey("MarkAsFavorite");
        properties.Should().NotContainKey("BuyCar");
        properties.Should().NotContainKey("Address");
    }

    private static string GetGeneratedSource(
        GeneratorDriverRunResult result,
        string htoClassName)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains($"{htoClassName}SirenMapper"));

        tree.Should().NotBeNull($"expected generated source for {htoClassName}");
        return tree!.GetText().ToString();
    }
}
