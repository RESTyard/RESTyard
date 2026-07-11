using System;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using RESTyard.Schema.SchemaGeneration;
using Xunit;

namespace RESTyard.Schema.Test;

public class JsonSchemaFactoryTests
{
    private readonly JsonSchemaFactory factory = new();

    [Fact]
    public void Obsolete_property_produces_deprecated_in_schema()
    {
        var schema = factory.Generate(typeof(TypeWithObsoleteProperty));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");
        var oldField = props.GetProperty("OldField");
        oldField.TryGetProperty("deprecated", out var deprecated).Should().BeTrue();
        deprecated.GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Non_obsolete_property_has_no_deprecated()
    {
        var schema = factory.Generate(typeof(TypeWithObsoleteProperty));
        var root = schema.RootElement;

        var props = root.GetProperty("properties");
        var newField = props.GetProperty("NewField");
        newField.TryGetProperty("deprecated", out _).Should().BeFalse();
    }

    [Fact]
    public void Obsolete_type_produces_deprecated_at_root()
    {
#pragma warning disable CS0618 // Type or member is obsolete
        var schema = factory.Generate(typeof(ObsoleteType));
#pragma warning restore CS0618 // Type or member is obsolete
        var root = schema.RootElement;

        root.TryGetProperty("deprecated", out var deprecated).Should().BeTrue();
        deprecated.GetBoolean().Should().BeTrue();
    }

    [Fact]
    public void Non_nullable_properties_are_required()
    {
        var schema = factory.Generate(typeof(TypeWithMixedNullability));
        var root = schema.RootElement;

        root.TryGetProperty("required", out var required).Should().BeTrue();
        var requiredNames = ToStringArray(required);
        requiredNames.Should().BeEquivalentTo("Name", "Count", "When");
    }

    [Fact]
    public void Nullable_properties_are_not_required()
    {
        var schema = factory.Generate(typeof(TypeWithMixedNullability));
        var requiredNames = ToStringArray(schema.RootElement.GetProperty("required"));

        requiredNames.Should().NotContain("OptionalName");
        requiredNames.Should().NotContain("OptionalCount");
    }

    [Fact]
    public void JsonPropertyName_override_is_used_in_required()
    {
        var schema = factory.Generate(typeof(TypeWithRenamedProperty));
        var requiredNames = ToStringArray(schema.RootElement.GetProperty("required"));

        requiredNames.Should().Contain("renamed");
        requiredNames.Should().NotContain("Original");
    }

    [Fact]
    public void Nested_complex_type_also_gets_required()
    {
        var schema = factory.Generate(typeof(TypeWithNestedComplexType));
        var root = schema.RootElement;

        ToStringArray(root.GetProperty("required")).Should().BeEquivalentTo("Nested");

        var defs = root.GetProperty("$defs");
        var nestedDef = defs.EnumerateObject().First().Value;
        ToStringArray(nestedDef.GetProperty("required")).Should().BeEquivalentTo("Inner");
    }

    [Fact]
    public void All_nullable_type_has_no_required()
    {
        var schema = factory.Generate(typeof(TypeWithOnlyNullableProperties));

        schema.RootElement.TryGetProperty("required", out _).Should().BeFalse();
    }

    [Fact]
    public void Derive_required_can_be_disabled()
    {
        var optOutFactory = new JsonSchemaFactory(deriveRequiredFromNonNullable: false);
        var schema = optOutFactory.Generate(typeof(TypeWithMixedNullability));

        schema.RootElement.TryGetProperty("required", out _).Should().BeFalse();
    }

    private static string[] ToStringArray(JsonElement array)
        => array.EnumerateArray().Select(e => e.GetString()!).ToArray();

    private class TypeWithMixedNullability
    {
        public string Name { get; set; } = string.Empty;

        public string? OptionalName { get; set; }

        public int Count { get; set; }

        public int? OptionalCount { get; set; }

        public DateTime When { get; set; }
    }

    private class TypeWithRenamedProperty
    {
        [System.Text.Json.Serialization.JsonPropertyName("renamed")]
        public string Original { get; set; } = string.Empty;
    }

    private class TypeWithNestedComplexType
    {
        public NestedType Nested { get; set; } = new();

        public NestedType? OptionalNested { get; set; }
    }

    private class NestedType
    {
        public string Inner { get; set; } = string.Empty;

        public string? OptionalInner { get; set; }
    }

    private class TypeWithOnlyNullableProperties
    {
        public string? MaybeName { get; set; }

        public int? MaybeCount { get; set; }
    }

    private class TypeWithObsoleteProperty
    {
        [Obsolete("Use NewField instead")]
        public string OldField { get; set; } = string.Empty;

        public string NewField { get; set; } = string.Empty;
    }

    [Obsolete("This type is deprecated")]
    private class ObsoleteType
    {
        public string Value { get; set; } = string.Empty;
    }
}
