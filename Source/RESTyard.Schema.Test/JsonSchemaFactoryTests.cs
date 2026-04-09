using System;
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
