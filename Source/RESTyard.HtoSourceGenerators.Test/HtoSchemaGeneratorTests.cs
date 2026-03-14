using System.Linq;
using AwesomeAssertions;
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
