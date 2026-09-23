using AwesomeAssertions;
using RESTyard.Schema.SchemaGeneration;
using Xunit;
using CM = System.ComponentModel;
using JS = Json.Schema.Generation;

namespace RESTyard.Schema.Test.SchemaGeneration;

/// <summary>
/// Both attribute families set title/description of runtime-generated schemas:
/// JsonSchema.Net (<c>[Title]</c>, <c>[Description]</c>) and <c>System.ComponentModel</c>
/// (<c>[DisplayName]</c>, <c>[Description]</c>).
/// </summary>
public class MetadataAttributeFamilyTests
{
    private readonly JsonSchemaFactory factory = new();

    [Fact]
    public void JsonSchemaNet_attributes_set_title_and_description()
    {
        var name = factory.Generate(typeof(WithJsonSchemaNet)).RootElement.GetProperty("properties").GetProperty("Name");

        name.GetProperty("title").GetString().Should().Be("Schema Title");
        name.GetProperty("description").GetString().Should().Be("Schema Description");
    }

    [Fact]
    public void ComponentModel_attributes_set_title_and_description()
    {
        var name = factory.Generate(typeof(WithComponentModel)).RootElement.GetProperty("properties").GetProperty("Name");

        name.GetProperty("title").GetString().Should().Be("BCL Title");
        name.GetProperty("description").GetString().Should().Be("BCL Description");
    }

    [Fact]
    public void JsonSchemaNet_attributes_win_over_ComponentModel_attributes()
    {
        var name = factory.Generate(typeof(WithBoth)).RootElement.GetProperty("properties").GetProperty("Name");

        name.GetProperty("title").GetString().Should().Be("Schema Title");
        name.GetProperty("description").GetString().Should().Be("Schema Description");
    }

    private class WithJsonSchemaNet
    {
        [JS.Title("Schema Title")]
        [JS.Description("Schema Description")]
        public string Name { get; set; } = string.Empty;
    }

    private class WithComponentModel
    {
        [CM.DisplayName("BCL Title")]
        [CM.Description("BCL Description")]
        public string Name { get; set; } = string.Empty;
    }

    private class WithBoth
    {
        [JS.Title("Schema Title")]
        [CM.DisplayName("BCL Title")]
        [JS.Description("Schema Description")]
        [CM.Description("BCL Description")]
        public string Name { get; set; } = string.Empty;
    }
}
