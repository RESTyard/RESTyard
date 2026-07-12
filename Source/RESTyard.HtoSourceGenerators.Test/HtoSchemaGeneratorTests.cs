using System;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using Microsoft.CodeAnalysis;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.RouteResolver;
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
            t.FilePath.Contains("HypermediaCustomerHtoSchema"));

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
            "TestHtos.HypermediaAddressHtoProperties.g.cs",
            "TestHtos.HypermediaAddressHtoSchema.g.cs",
            "TestHtos.HypermediaCustomerHtoProperties.g.cs",
            "TestHtos.HypermediaCustomerHtoSchema.g.cs",
            "HypermediaSchemaRegistry.g.cs");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.FullHto);
    }

    [Fact]
    public void Hto_without_title_omits_title_in_schema()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

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

            [assembly: HypermediaAssembly]

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

            [assembly: HypermediaAssembly]

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

            [assembly: HypermediaAssembly]

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
    public void SimpleHto_calls_factory_with_properties_poco()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        // Method takes IJsonSchemaFactory parameter
        source.Should().Contain("GetSchema(IJsonSchemaFactory schemaFactory)");

        // Calls factory with the generated properties POCO type
        source.Should().Contain("schemaFactory.Generate(typeof(HypermediaCustomerHtoProperties))");
        source.Should().Contain("PropertiesSchema = propertiesSchema");
    }

    [Fact]
    public void Hto_with_various_property_types_generates_poco_and_compiles()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithVariousPropertyTypes);
        var source = GetGeneratedSource(result, "HypermediaAllTypesHto");

        // Uses the generated POCO for schema generation
        source.Should().Contain("schemaFactory.Generate(typeof(HypermediaAllTypesHtoProperties))");

        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithVariousPropertyTypes);
    }

    [Fact]
    public void Nullable_properties_generate_poco_and_compile()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithNullableProperties);
        var source = GetGeneratedSource(result, "HypermediaNullableHto");

        source.Should().Contain("schemaFactory.Generate(typeof(HypermediaNullableHtoProperties))");
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithNullableProperties);
    }

    [Fact]
    public void Nullable_reference_annotation_is_preserved_in_poco()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public string? Nickname { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var pocoTree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains("HypermediaCustomerHtoProperties.g.cs"));
        pocoTree.Should().NotBeNull();
        var poco = pocoTree!.GetText().ToString();

        poco.Should().Contain("public string Name");
        poco.Should().Contain("public string? Nickname");
    }

    [Fact]
    public void Non_nullable_properties_are_required_in_generated_schema()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public string? Nickname { get; set; }

                public int Age { get; set; }

                public int? ShoeSize { get; set; }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaCustomerHto", source);

        schema.PropertiesSchema.Should().NotBeNull();
        var root = schema.PropertiesSchema!.RootElement;
        root.TryGetProperty("required", out var required).Should().BeTrue();
        var requiredNames = required.EnumerateArray().Select(e => e.GetString()).ToArray();
        requiredNames.Should().BeEquivalentTo("Name", "Age");
    }

    [Fact]
    public void Enum_properties_generate_poco_and_compile()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithEnumProperties);
    }

    [Fact]
    public void Collection_and_array_properties_generate_poco_and_compile()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithCollections);
    }

    [Fact]
    public void Nested_object_property_generates_poco_and_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithNestedObject);
    }

    [Fact]
    public void HypermediaProperty_Name_renames_and_FormatterIgnore_excludes_in_schema()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithPropertyAttributes);
        var source = GetGeneratedSource(result, "HypermediaWithAttributesHto");

        // Uses POCO for schema generation
        source.Should().Contain("schemaFactory.Generate(typeof(HypermediaWithAttributesHtoProperties))");

        // [FormatterIgnoreHypermediaProperty] should exclude the property from the POCO
        var poco = GetGeneratedPoco(result, "HypermediaWithAttributesHto");
        poco.Should().Contain("public string FullName { get; set; }");
        poco.Should().NotContain("InternalId");
        poco.Should().Contain("public int Age { get; set; }");
    }

    [Fact]
    public void Links_and_actions_are_excluded_from_properties_poco()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);
        var poco = GetGeneratedPoco(result, "HypermediaCustomerHto");

        // Data properties should be present
        poco.Should().Contain("public string Name { get; set; }");
        poco.Should().Contain("public int Age { get; set; }");

        // Links, actions, embedded entities should not appear
        poco.Should().NotContain("Self");
        poco.Should().NotContain("BestFriend");
        poco.Should().NotContain("MarkAsFavorite");
        poco.Should().NotContain("BuyCar");
        poco.Should().NotContain("Address");
    }

    [Fact]
    public void Hto_without_properties_omits_PropertiesSchema_and_factory_parameter()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

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
        HtoMetadataExtractor.DeriveSchemaName(className).Should().Be(expected);
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

        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");
        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("Age", out _).Should().BeTrue();

        props.GetProperty("Name").GetProperty("type").GetString().Should().Be("string");
        props.GetProperty("Age").GetProperty("type").GetString().Should().Be("integer");
    }

    [Fact]
    public void FullHto_GetSchema_excludes_links_actions_includes_data_properties()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.FullHto);

        schema.Name.Should().Be("Customer");
        schema.PropertiesSchema.Should().NotBeNull();

        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");

        // Data properties should be present
        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("Age", out _).Should().BeTrue();

        // Links, actions, and embedded entities should be excluded
        props.TryGetProperty("Self", out _).Should().BeFalse();
        props.TryGetProperty("BestFriend", out _).Should().BeFalse();
        props.TryGetProperty("MarkAsFavorite", out _).Should().BeFalse();
        props.TryGetProperty("BuyCar", out _).Should().BeFalse();
        props.TryGetProperty("Address", out _).Should().BeFalse();
    }

    [Fact]
    public void HtoWithLinks_generates_links_array_in_source()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithLinks);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        source.Should().Contain("Links = new LinkDescription[]");
        source.Should().Contain("Relations = new[] { \"self\" }");
        source.Should().Contain("Relations = new[] { \"bestFriend\" }");
        source.Should().Contain("TargetName = \"Customer\"");
        source.Should().Contain("TargetClasses = new[] { \"Customer\" }");
    }

    [Fact]
    public void HtoWithLinks_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithLinks);
    }

    [Fact]
    public void HtoWithLinks_GetSchema_returns_correct_links()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithLinks);

        schema.Links.Should().HaveCount(2);

        var selfLink = schema.Links.Single(l => l.Relations.Contains("self"));
        selfLink.TargetName.Should().Be("Customer");
        selfLink.TargetClasses.Should().BeEquivalentTo("Customer");
        selfLink.IsExternal.Should().BeFalse();
        selfLink.IsMandatory.Should().BeTrue();

        var bestFriendLink = schema.Links.Single(l => l.Relations.Contains("bestFriend"));
        bestFriendLink.TargetName.Should().Be("Customer");
        bestFriendLink.TargetClasses.Should().BeEquivalentTo("Customer");
        bestFriendLink.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void HtoWithLinks_to_different_target_resolves_target_metadata()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Order", Classes = ["Order", "Document"])]
            public class HypermediaOrderHto : HypermediaObject
            {
                public string OrderNumber { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                [Relations(["latestOrder"])]
                public ILink<HypermediaOrderHto>? LatestOrder { get; set; }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", source);

        schema.Links.Should().ContainSingle();
        var link = schema.Links[0];
        link.Relations.Should().BeEquivalentTo("latestOrder");
        link.TargetName.Should().Be("Order");
        link.TargetClasses.Should().BeEquivalentTo("Order", "Document");
        link.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void Hto_without_links_has_empty_links_collection()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.SimpleHto);

        schema.Links.Should().BeEmpty();
    }

    private const string HtoWithExternalLinks = """
        using System;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.AspNetCore.Hypermedia.Links;
        using RESTyard.Schema.Model;

        [assembly: HypermediaAssembly(Siren = true)]

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Relations(["invoice-pdf"])]
            [HypermediaMediaType("application/pdf", "text/html")]
            public ExternalLink Invoice { get; set; } = Link.External(
                new HypermediaObjectReference(new ExternalReference(new Uri("https://example.com/invoice.pdf"))));

            [Relations(["website"])]
            public ExternalLink? Website { get; set; }
        }
        """;

    [Fact]
    public void ExternalLink_with_relations_is_included_in_schema_links()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", HtoWithExternalLinks);

        schema.Links.Should().HaveCount(2);

        var invoiceLink = schema.Links.Single(l => l.Relations.Contains("invoice-pdf"));
        invoiceLink.TargetName.Should().BeNull();
        invoiceLink.TargetClasses.Should().BeEmpty();
        invoiceLink.IsExternal.Should().BeTrue();
        invoiceLink.MediaTypes.Should().BeEquivalentTo("application/pdf", "text/html");
        invoiceLink.IsMandatory.Should().BeTrue();

        var websiteLink = schema.Links.Single(l => l.Relations.Contains("website"));
        websiteLink.TargetName.Should().BeNull();
        websiteLink.IsExternal.Should().BeTrue();
        // No [HypermediaMediaType] declared — defaults to the Siren media type
        websiteLink.MediaTypes.Should().BeEquivalentTo("application/vnd.siren+json");
        websiteLink.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void ExternalLink_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(HtoWithExternalLinks);
    }

    [Fact]
    public void ExternalLink_is_excluded_from_properties_poco()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", HtoWithExternalLinks);

        schema.PropertiesSchema.Should().NotBeNull();
        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");
        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("Invoice", out _).Should().BeFalse();
        props.TryGetProperty("Website", out _).Should().BeFalse();
    }

    [Fact]
    public void ExternalLink_without_relations_emits_RY0021_warning()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public ExternalLink? MissingRelExternalLink { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var customerDiags = result.Diagnostics
            .Where(d => d.GetMessage().Contains("HypermediaCustomerHto"))
            .ToArray();
        customerDiags.Should().ContainSingle();
        customerDiags[0].Id.Should().Be("RY0021");
        customerDiags[0].GetMessage().Should().Contain("MissingRelExternalLink");
        customerDiags[0].Severity.Should().Be(DiagnosticSeverity.Warning);
    }

    [Fact]
    public void ExternalLink_appears_in_generated_siren_output()
    {
        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));

        var json = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options: null, HtoWithExternalLinks);

        json.Should().Contain("invoice-pdf");
        // Nullable external link left null is omitted
        json.Should().NotContain("website");
    }

    [Fact]
    public void Link_type_falls_back_to_declared_media_types_and_is_omitted_otherwise()
    {
        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));

        var json = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options: null, HtoWithExternalLinks);

        using var doc = JsonDocument.Parse(json);
        var links = doc.RootElement.GetProperty("links").EnumerateArray().ToList();

        // No runtime media types on the reference → declared [HypermediaMediaType] is used
        var invoice = links.Single(l => l.GetProperty("rel")[0].GetString() == "invoice-pdf");
        invoice.GetProperty("type").GetString().Should().Be("application/pdf,text/html");

        // Plain link without runtime or declared media types → no type on the wire
        // (Siren is the baseline; the schema states the default via mediaTypes)
        var self = links.Single(l => l.GetProperty("rel")[0].GetString() == "self");
        self.TryGetProperty("type", out _).Should().BeFalse();
    }

    private const string HtoWithRuntimeMediaType = """
        using System;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.AspNetCore.Hypermedia.Links;
        using RESTyard.Schema.Model;

        [assembly: HypermediaAssembly(Siren = true)]

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            [Relations(["invoice-pdf"])]
            [HypermediaMediaType("application/pdf")]
            public ExternalLink Invoice { get; set; } = Link.External(
                new HypermediaObjectReference(
                    new ExternalReference(new Uri("https://example.com/invoice.pdf"))
                        .WithAvailableMediaType("text/csv")));
        }
        """;

    [Fact]
    public void Runtime_media_type_wins_over_declared_and_mismatch_warns_by_default()
    {
        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));
        var warnings = new System.Collections.Generic.List<string>();
        var options = new RESTyard.AspNetCore.Hypermedia.Siren.SirenMapperOptions
        {
            MediaTypeMismatchWarningHandler = warnings.Add,
        };

        var json = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options, HtoWithRuntimeMediaType);

        using var doc = JsonDocument.Parse(json);
        var invoice = doc.RootElement.GetProperty("links").EnumerateArray()
            .Single(l => l.GetProperty("rel")[0].GetString() == "invoice-pdf");
        invoice.GetProperty("type").GetString().Should().Be("text/csv");

        warnings.Should().ContainSingle();
        warnings[0].Should().Contain("Invoice").And.Contain("text/csv").And.Contain("application/pdf");
    }

    [Fact]
    public void Media_type_mismatch_with_Throw_behavior_throws()
    {
        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));
        var options = new RESTyard.AspNetCore.Hypermedia.Siren.SirenMapperOptions
        {
            MediaTypeMismatch = RESTyard.AspNetCore.Hypermedia.Siren.MediaTypeMismatchBehavior.Throw,
        };

        var act = () => GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options, HtoWithRuntimeMediaType);

        // Reflection invoke wraps the InvalidOperationException from the generated mapper
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*Invoice*text/csv*");
    }

    [Fact]
    public void Media_type_mismatch_with_Ignore_behavior_is_silent()
    {
        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));
        var warnings = new System.Collections.Generic.List<string>();
        var options = new RESTyard.AspNetCore.Hypermedia.Siren.SirenMapperOptions
        {
            MediaTypeMismatch = RESTyard.AspNetCore.Hypermedia.Siren.MediaTypeMismatchBehavior.Ignore,
            MediaTypeMismatchWarningHandler = warnings.Add,
        };

        GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options, HtoWithRuntimeMediaType);

        warnings.Should().BeEmpty();
    }

    [Fact]
    public void Runtime_media_type_within_declared_list_does_not_warn()
    {
        const string source = """
            using System;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.Hypermedia.Links;
            using RESTyard.Schema.Model;

            [assembly: HypermediaAssembly(Siren = true)]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                [Relations(["invoice-pdf"])]
                [HypermediaMediaType("application/pdf", "text/html")]
                public ExternalLink Invoice { get; set; } = Link.External(
                    new HypermediaObjectReference(
                        new ExternalReference(new Uri("https://example.com/invoice.pdf"))
                            .WithAvailableMediaType("application/pdf")));
            }
            """;

        var resolver = new StubRouteResolver(
            new RESTyard.AspNetCore.WebApi.RouteResolver.ResolvedRoute("http://test/self", "GET"));
        var warnings = new System.Collections.Generic.List<string>();
        var options = new RESTyard.AspNetCore.Hypermedia.Siren.SirenMapperOptions
        {
            MediaTypeMismatchWarningHandler = warnings.Add,
        };

        var json = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options, source);

        using var doc = JsonDocument.Parse(json);
        var invoice = doc.RootElement.GetProperty("links").EnumerateArray()
            .Single(l => l.GetProperty("rel")[0].GetString() == "invoice-pdf");
        invoice.GetProperty("type").GetString().Should().Be("application/pdf");

        warnings.Should().BeEmpty();
    }

    [Fact]
    public void HtoWithActions_generates_actions_array_in_source()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithActions);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        source.Should().Contain("Actions = new ActionDescription[]");
        source.Should().Contain("Name = \"MarkAsFavorite\"");
        source.Should().Contain("Name = \"BuyCar\"");
    }

    [Fact]
    public void HtoWithActions_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithActions);
    }

    [Fact]
    public void HtoWithActions_GetSchema_returns_correct_actions()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithActions);

        schema.Actions.Should().HaveCount(2);

        var markAsFavorite = schema.Actions.Single(a => a.Name == "MarkAsFavorite");
        markAsFavorite.ParameterSchema.Should().BeNull();
        markAsFavorite.IsMandatory.Should().BeFalse();
        markAsFavorite.IsFileUpload.Should().BeFalse();

        var buyCar = schema.Actions.Single(a => a.Name == "BuyCar");
        buyCar.ParameterSchema.Should().NotBeNull();
        buyCar.IsMandatory.Should().BeFalse();
        buyCar.IsFileUpload.Should().BeFalse();
    }

    [Fact]
    public void HtoWithActions_parameterized_action_has_schema_with_properties()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithActions);

        var buyCar = schema.Actions.Single(a => a.Name == "BuyCar");
        buyCar.ParameterSchema.Should().NotBeNull();

        var paramProps = buyCar.ParameterSchema!.RootElement.GetProperty("properties");
        paramProps.TryGetProperty("CarId", out _).Should().BeTrue();
    }

    [Fact]
    public void HtoWithActions_custom_name_and_title_from_attribute()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class DoSomethingAction : HypermediaAction
            {
                public DoSomethingAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Thing", Classes = ["Thing"])]
            public class HypermediaThingHto : HypermediaObject
            {
                [HypermediaAction(Name = "CustomName", Title = "Custom Title")]
                public DoSomethingAction? DoIt { get; set; }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaThingHto", source);

        schema.Actions.Should().ContainSingle();
        var action = schema.Actions[0];
        action.Name.Should().Be("CustomName");
        action.Title.Should().Be("Custom Title");
    }

    [Fact]
    public void HtoWithFileUpload_generates_file_upload_action()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaDocumentHto", TestHtoSources.HtoWithFileUpload);

        schema.Actions.Should().ContainSingle();
        var action = schema.Actions[0];
        action.Name.Should().Be("Upload");
        action.Title.Should().Be("Upload File");
        action.IsFileUpload.Should().BeTrue();
        action.ContentType.Should().Be("multipart/form-data");
        action.ParameterSchema.Should().BeNull();
        action.IsMandatory.Should().BeFalse();
    }

    [Fact]
    public void HtoWithFileUpload_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithFileUpload);
    }

    [Fact]
    public void Hto_without_actions_has_empty_actions_collection()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.SimpleHto);

        schema.Actions.Should().BeEmpty();
    }

    [Fact]
    public void HtoWithActions_only_parameterized_action_still_gets_schema_factory()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class SearchParameters : IHypermediaActionParameter
            {
                public string Query { get; set; } = string.Empty;
            }

            public class SearchAction : HypermediaAction<SearchParameters>
            {
                public SearchAction() : base() { }
            }

            [HypermediaObject(Title = "Search", Classes = ["Search"])]
            public class HypermediaSearchHto : HypermediaObject
            {
                [HypermediaAction(Name = "Search")]
                public SearchAction? Search { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var generated = GetGeneratedSource(result, "HypermediaSearchHto");

        // No data properties, but has parameterized action -> needs factory
        generated.Should().Contain("GetSchema(IJsonSchemaFactory schemaFactory)");
        generated.Should().Contain("schemaFactory.Generate(typeof(");
    }

    [Fact]
    public void HtoWithEmbedded_generates_embedded_entities_array_in_source()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithEmbedded);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        source.Should().Contain("EmbeddedEntities = new EmbeddedEntityDescription[]");
        source.Should().Contain("Relations = new[] { \"address\" }");
        source.Should().Contain("Relations = new[] { \"addresses\" }");
        source.Should().Contain("TargetName = \"Address\"");
        source.Should().Contain("TargetClasses = new[] { \"Address\" }");
    }

    [Fact]
    public void HtoWithEmbedded_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithEmbedded);
    }

    [Fact]
    public void HtoWithEmbedded_GetSchema_returns_correct_embedded_entities()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithEmbedded);

        schema.EmbeddedEntities.Should().HaveCount(2);

        var address = schema.EmbeddedEntities.Single(e => e.Relations.Contains("address"));
        address.TargetName.Should().Be("Address");
        address.TargetClasses.Should().BeEquivalentTo("Address");
        address.IsCollection.Should().BeFalse();
        address.IsMandatory.Should().BeFalse();

        var addresses = schema.EmbeddedEntities.Single(e => e.Relations.Contains("addresses"));
        addresses.TargetName.Should().Be("Address");
        addresses.TargetClasses.Should().BeEquivalentTo("Address");
        addresses.IsCollection.Should().BeTrue();
        addresses.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void HtoWithEmbedded_to_different_target_resolves_target_metadata()
    {
        const string source = """
            using System.Collections.Generic;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Order", Classes = ["Order", "Document"])]
            public class HypermediaOrderHto : HypermediaObject
            {
                public string OrderNumber { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                [Relations(["orders"])]
                public List<IEmbeddedEntity<HypermediaOrderHto>> Orders { get; set; } = new();
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", source);

        schema.EmbeddedEntities.Should().ContainSingle();
        var embedded = schema.EmbeddedEntities[0];
        embedded.Relations.Should().BeEquivalentTo("orders");
        embedded.TargetName.Should().Be("Order");
        embedded.TargetClasses.Should().BeEquivalentTo("Order", "Document");
        embedded.IsCollection.Should().BeTrue();
        embedded.IsMandatory.Should().BeTrue();
    }

    [Fact]
    public void Hto_without_embedded_entities_has_empty_collection()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.SimpleHto);

        schema.EmbeddedEntities.Should().BeEmpty();
    }

    [Fact]
    public void Embedded_entities_are_excluded_from_properties_schema()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithEmbedded);

        schema.PropertiesSchema.Should().NotBeNull();
        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");

        // Data property should be present
        props.TryGetProperty("Name", out _).Should().BeTrue();

        // Embedded entities should not appear in properties
        props.TryGetProperty("Address", out _).Should().BeFalse();
        props.TryGetProperty("Addresses", out _).Should().BeFalse();
    }

    [Fact]
    public void Embedded_entity_without_relations_emits_RY0020_warning()
    {
        const string source = """
            using System.Collections.Generic;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Address", Classes = ["Address"])]
            public class HypermediaAddressHto : HypermediaObject
            {
                public string Street { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public IEmbeddedEntity<HypermediaAddressHto>? MissingRelations { get; set; }

                public List<IEmbeddedEntity<HypermediaAddressHto>> AlsoMissing { get; set; } = new();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().HaveCount(2);
        result.Diagnostics.Should().OnlyContain(d => d.Id == "RY0020");
        result.Diagnostics.Should().Contain(d => d.GetMessage(null).Contains("MissingRelations"));
        result.Diagnostics.Should().Contain(d => d.GetMessage(null).Contains("AlsoMissing"));

        // GEN-12: message says "will be ignored" — a warning, not an error.
        // GEN-13: the diagnostic points at the property, not Location.None.
        result.Diagnostics.Should().OnlyContain(d => d.Severity == DiagnosticSeverity.Warning);
        result.Diagnostics.Should().OnlyContain(d => d.Location != Location.None);
    }

    [Fact]
    public void Link_without_relations_emits_RY0021_warning()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Other", Classes = ["Other"])]
            public class HypermediaOtherHto : HypermediaObject { }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public ILink<HypermediaOtherHto>? MissingRelLink { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var customerDiags = result.Diagnostics
            .Where(d => d.GetMessage().Contains("HypermediaCustomerHto"))
            .ToArray();
        customerDiags.Should().ContainSingle();
        customerDiags[0].Id.Should().Be("RY0021");
        customerDiags[0].GetMessage().Should().Contain("MissingRelLink");
        customerDiags[0].Severity.Should().Be(DiagnosticSeverity.Warning);
        customerDiags[0].Location.Should().NotBe(Location.None);
    }

    [Fact]
    public void Link_without_relations_is_excluded_from_properties()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Other", Classes = ["Other"])]
            public class HypermediaOtherHto : HypermediaObject { }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public ILink<HypermediaOtherHto>? OrphanLink { get; set; }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", source);

        schema.PropertiesSchema.Should().NotBeNull();
        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");
        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("OrphanLink", out _).Should().BeFalse();

        schema.Links.Should().BeEmpty();
    }

    [Fact]
    public void Embedded_entity_without_relations_is_excluded_from_properties()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Address", Classes = ["Address"])]
            public class HypermediaAddressHto : HypermediaObject
            {
                public string Street { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                public IEmbeddedEntity<HypermediaAddressHto>? Orphan { get; set; }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", source);

        schema.PropertiesSchema.Should().NotBeNull();
        var props = schema.PropertiesSchema!.RootElement.GetProperty("properties");
        props.TryGetProperty("Name", out _).Should().BeTrue();
        props.TryGetProperty("Orphan", out _).Should().BeFalse();

        schema.EmbeddedEntities.Should().BeEmpty();
    }

    [Fact]
    public void FullHto_GetSchema_has_embedded_entity()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.FullHto);

        schema.EmbeddedEntities.Should().ContainSingle();
        var embedded = schema.EmbeddedEntities[0];
        embedded.Relations.Should().BeEquivalentTo("address");
        embedded.TargetName.Should().Be("Address");
        embedded.IsCollection.Should().BeFalse();
        embedded.IsMandatory.Should().BeFalse();
    }

    // --- Step 2.7: Title and description harvesting ---

    [Fact]
    public void HtoWithTitleDescriptionAttributes_entity_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithTitleDescriptionAttributes);

        schema.Title.Should().Be("Customer Entity");
        schema.Description.Should().Be("Represents a customer in the system.");
    }

    [Fact]
    public void HtoWithTitleDescriptionAttributes_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithTitleDescriptionAttributes);
    }

    [Fact]
    public void HtoWithTitleDescriptionAttributes_link_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithTitleDescriptionAttributes);

        var link = schema.Links.Single(l => l.Relations.Contains("bestFriend"));
        link.Title.Should().Be("Best Friend Link");
        link.Description.Should().Be("Link to the customer's best friend.");
    }

    [Fact]
    public void HtoWithTitleDescriptionAttributes_action_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithTitleDescriptionAttributes);

        var action = schema.Actions.Single(a => a.Name == "MarkAsFavorite");
        action.Title.Should().Be("Mark As Favorite");
        action.Description.Should().Be("Marks this customer as a favorite.");
    }

    [Fact]
    public void HtoWithTitleDescriptionAttributes_embedded_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithTitleDescriptionAttributes);

        var embedded = schema.EmbeddedEntities.Single(e => e.Relations.Contains("address"));
        embedded.Title.Should().Be("Home Address");
        embedded.Description.Should().Be("The customer's home address.");
    }

    [Fact]
    public void HtoWithXmlDocs_entity_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithXmlDocs);

        schema.Title.Should().Be("A customer with profile and order history.");
        schema.Description.Should().Be("Represents an active customer account in the system.");
    }

    [Fact]
    public void HtoWithXmlDocs_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithXmlDocs);
    }

    [Fact]
    public void HtoWithXmlDocs_link_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithXmlDocs);

        var link = schema.Links.Single(l => l.Relations.Contains("bestFriend"));
        link.Title.Should().Be("Link to the customer's best friend.");
        link.Description.Should().Be("Only present when a best friend is set.");
    }

    [Fact]
    public void HtoWithXmlDocs_action_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithXmlDocs);

        var action = schema.Actions.Single(a => a.Name == "MarkAsFavorite");
        action.Title.Should().Be("Marks this customer as a favorite.");
        action.Description.Should().Be("Can only be executed by admins.");
    }

    [Fact]
    public void HtoWithXmlDocs_embedded_has_title_and_description()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithXmlDocs);

        var embedded = schema.EmbeddedEntities.Single(e => e.Relations.Contains("address"));
        embedded.Title.Should().Be("The customer's home address.");
        embedded.Description.Should().Be("Primary residential address.");
    }

    [Fact]
    public void HtoWithAttributeOverridingXmlDocs_attribute_wins()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAttributeOverridingXmlDocs);

        schema.Title.Should().Be("Attribute Title");
        schema.Description.Should().Be("Attribute Description");
    }

    [Fact]
    public void HtoWithAttributeOverridingXmlDocs_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithAttributeOverridingXmlDocs);
    }

    [Fact]
    public void HtoWithHypermediaObjectTitle_overrides_TitleAttribute()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithHypermediaObjectTitleOverridingTitleAttribute);

        schema.Title.Should().Be("HypermediaObject Title");
    }

    [Fact]
    public void HtoWithHypermediaObjectTitle_overrides_TitleAttribute_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithHypermediaObjectTitleOverridingTitleAttribute);
    }

    [Fact]
    public void HtoWithTitleDescriptionAttributes_generates_title_description_in_source()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithTitleDescriptionAttributes);
        var source = GetGeneratedSource(result, "HypermediaCustomerHto");

        source.Should().Contain("Title = \"Customer Entity\"");
        source.Should().Contain("Description = \"Represents a customer in the system.\"");
    }

    // --- Step 2.13: Action ResultType ---

    [Fact]
    public void Action_with_ResultType_populates_ResultName()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class CreateQueryAction : HypermediaAction
            {
                public CreateQueryAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "CreateQuery")]
                public CreateQueryAction? CreateQuery { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("query")]
                [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult CreateQuery() => Ok();
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaRootHto", source);

        schema.Actions.Should().ContainSingle();
        var action = schema.Actions[0];
        action.Name.Should().Be("CreateQuery");
        action.ResultName.Should().Be("QueryResult");
        action.ResultClasses.Should().BeEquivalentTo("QueryResult");
    }

    [Fact]
    public void Action_with_ResultType_on_nested_controller_populates_ResultName()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class CreateQueryAction : HypermediaAction
            {
                public CreateQueryAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "CreateQuery")]
                public CreateQueryAction? CreateQuery { get; set; }
            }

            public static class Endpoints
            {
                [ApiController]
                [Route("api")]
                public class RootController : ControllerBase
                {
                    [HttpPost("query")]
                    [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery",
                        ResultType = typeof(HypermediaQueryResultHto))]
                    public IActionResult CreateQuery() => Ok();
                }
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaRootHto", source);

        schema.Actions.Should().ContainSingle();
        schema.Actions[0].ResultName.Should().Be("QueryResult");
    }

    [Fact]
    public void Action_with_ResultType_not_HypermediaObject_emits_RY0032_warning()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class NotAnHto
            {
                public string Data { get; set; } = string.Empty;
            }

            public class SomeAction : HypermediaAction
            {
                public SomeAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "DoStuff")]
                public SomeAction? DoStuff { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("do")]
                [HypermediaActionEndpoint<HypermediaRootHto>("DoStuff",
                    ResultType = typeof(NotAnHto))]
                public IActionResult Do() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.Diagnostics.Should().Contain(d => d.Id == "RY0032");
    }

    [Fact]
    public void Action_with_201_but_no_ResultType_emits_RY0031_warning()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class SomeAction : HypermediaAction
            {
                public SomeAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "Create")]
                public SomeAction? Create { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("create")]
                [ProducesResponseType(201)]
                [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
                public IActionResult Create() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.Diagnostics.Should().Contain(d => d.Id == "RY0031");
    }

    [Fact]
    public void Action_without_ResultType_has_null_ResultName()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithActions);

        foreach (var action in schema.Actions)
        {
            action.ResultName.Should().BeNull();
            action.ResultClasses.Should().BeNull();
        }
    }

    [Fact]
    public void Action_with_renamed_action_and_ResultType_populates_ResultName()
    {
        // GEN-02: the endpoint attribute names the C# property ("CreateQuery"), while the
        // schema action name is overridden via [HypermediaAction(Name = "startQuery")].
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class CreateQueryAction : HypermediaAction
            {
                public CreateQueryAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "startQuery")]
                public CreateQueryAction? CreateQuery { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("query")]
                [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult CreateQuery() => Ok();
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaRootHto", source);

        schema.Actions.Should().ContainSingle();
        var action = schema.Actions[0];
        action.Name.Should().Be("startQuery");
        action.ResultName.Should().Be("QueryResult");
        action.ResultClasses.Should().BeEquivalentTo("QueryResult");
    }

    [Fact]
    public void Inherited_action_on_derived_hto_gets_ResultName()
    {
        // GEN-17: the endpoint attribute names the base HTO; derived HTOs inherit the
        // action property and must carry the same result information.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class CreateQueryAction : HypermediaAction
            {
                public CreateQueryAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Base", Classes = ["Base"])]
            public class HypermediaBaseHto : HypermediaObject
            {
                [HypermediaAction(Name = "CreateQuery")]
                public CreateQueryAction? CreateQuery { get; set; }
            }

            [HypermediaObject(Title = "Derived", Classes = ["Derived"])]
            public class HypermediaDerivedHto : HypermediaBaseHto
            {
            }

            [HypermediaObject(Title = "NextLevel", Classes = ["NextLevel"])]
            public class HypermediaNextLevelHto : HypermediaDerivedHto
            {
            }

            [ApiController]
            [Route("api")]
            public class BaseController : ControllerBase
            {
                [HttpPost("query")]
                [HypermediaActionEndpoint<HypermediaBaseHto>("CreateQuery",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult CreateQuery() => Ok();
            }
            """;

        foreach (var htoClassName in new[] { "HypermediaBaseHto", "HypermediaDerivedHto", "HypermediaNextLevelHto" })
        {
            var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(htoClassName, source);

            schema.Actions.Should().ContainSingle();
            schema.Actions[0].ResultName.Should().Be("QueryResult",
                $"the inherited action on {htoClassName} should resolve the mapping declared for the base HTO");
            schema.Actions[0].ResultClasses.Should().BeEquivalentTo("QueryResult");
        }
    }

    [Fact]
    public void ResultType_warnings_are_reported_once_regardless_of_hto_count()
    {
        // GEN-03: RY0031/RY0032 come from a compilation-level diagnostics output,
        // not the per-HTO output — multiple HTOs must not duplicate them.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class NotAnHto
            {
                public string Data { get; set; } = string.Empty;
            }

            public class SomeAction : HypermediaAction
            {
                public SomeAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "First", Classes = ["First"])]
            public class HypermediaFirstHto : HypermediaObject
            {
                [HypermediaAction(Name = "DoStuff")]
                public SomeAction? DoStuff { get; set; }
            }

            [HypermediaObject(Title = "Second", Classes = ["Second"])]
            public class HypermediaSecondHto : HypermediaObject
            {
                public string Value { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Third", Classes = ["Third"])]
            public class HypermediaThirdHto : HypermediaObject
            {
                public string Value { get; set; } = string.Empty;
            }

            [ApiController]
            [Route("api")]
            public class FirstController : ControllerBase
            {
                [HttpPost("do")]
                [HypermediaActionEndpoint<HypermediaFirstHto>("DoStuff",
                    ResultType = typeof(NotAnHto))]
                public IActionResult Do() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Where(d => d.Id == "RY0032").Should().ContainSingle();
    }

    [Fact]
    public void ResultType_warnings_are_not_reported_without_HypermediaAssembly()
    {
        // GEN-03: no [assembly: HypermediaAssembly] → the generator emits nothing,
        // including schema diagnostics.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            namespace TestHtos;

            public class NotAnHto
            {
                public string Data { get; set; } = string.Empty;
            }

            public class SomeAction : HypermediaAction
            {
                public SomeAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "DoStuff")]
                public SomeAction? DoStuff { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("do")]
                [ProducesResponseType(201)]
                [HypermediaActionEndpoint<HypermediaRootHto>("DoStuff",
                    ResultType = typeof(NotAnHto))]
                public IActionResult Do() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().NotContain(d => d.Id == "RY0031" || d.Id == "RY0032");
        result.GeneratedTrees.Should().BeEmpty();
    }

    // --- Multi-assembly ResultType support (GEN-01) ---

    private const string ReferencedHtoAssemblySource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;

        namespace ReferencedHtos;

        [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
        public class HypermediaQueryResultHto : HypermediaObject
        {
            public string ResultData { get; set; } = string.Empty;
        }

        public class CreateQueryAction : HypermediaAction
        {
            public CreateQueryAction() : base(() => true) { }
        }

        [HypermediaObject(Title = "Root", Classes = ["Root"])]
        public class HypermediaRootHto : HypermediaObject
        {
            [HypermediaAction(Name = "startQuery")]
            public CreateQueryAction? CreateQuery { get; set; }
        }
        """;

    [Fact]
    public void Controller_only_assembly_emits_action_result_registry()
    {
        // GEN-01: controllers here, HTOs in a referenced assembly — the exact scenario
        // the action-result registry exists for. It must be emitted despite zero local HTOs,
        // carry the runtime discovery attribute, and use schema-level names
        // (derived entity name, [HypermediaAction(Name)] override resolved from metadata).
        const string controllerSource = """
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using ReferencedHtos;
            using Microsoft.AspNetCore.Mvc;

            [assembly: RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaAssembly]

            namespace Controllers;

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("query")]
                [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult CreateQuery() => Ok();
            }
            """;

        var htoAssembly = GeneratorTestHelper.CompileToMetadataReference(
            "ReferencedHtoAssembly", ReferencedHtoAssemblySource);

        var result = GeneratorTestHelper.RunGenerator([htoAssembly], controllerSource);

        var registry = result.GeneratedTrees
            .Should().ContainSingle(t => t.FilePath.EndsWith("HypermediaActionResultRegistry.g.cs"))
            .Which.ToString();

        registry.Should().Contain(
            "[assembly: global::RESTyard.Schema.Model.HypermediaActionResultRegistryAttribute(typeof(HypermediaActionResultRegistry_TestAssembly))]");
        registry.Should().Contain("EntityName = \"Root\"");
        registry.Should().Contain("ActionName = \"startQuery\"");
        registry.Should().Contain("ResultName = \"QueryResult\"");
        registry.Should().Contain("\"QueryResult\"");
    }

    [Fact]
    public void Controller_only_assembly_reports_ResultType_warnings()
    {
        // GEN-03: schema diagnostics must also fire in assemblies without any HTO.
        const string controllerSource = """
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using ReferencedHtos;
            using Microsoft.AspNetCore.Mvc;

            [assembly: RESTyard.AspNetCore.Hypermedia.Attributes.HypermediaAssembly]

            namespace Controllers;

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("query")]
                [ProducesResponseType(201)]
                [HypermediaActionEndpoint<HypermediaRootHto>("CreateQuery")]
                public IActionResult CreateQuery() => Ok();
            }
            """;

        var htoAssembly = GeneratorTestHelper.CompileToMetadataReference(
            "ReferencedHtoAssembly", ReferencedHtoAssemblySource);

        var result = GeneratorTestHelper.RunGenerator([htoAssembly], controllerSource);

        result.Diagnostics.Where(d => d.Id == "RY0031").Should().ContainSingle();
    }

    // --- Same class name in two namespaces (GEN-05) ---

    private const string SameClassNameInTwoNamespacesSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.AspNetCore.WebApi.AttributedRoutes;
        using Microsoft.AspNetCore.Mvc;

        [assembly: HypermediaAssembly]

        namespace NsA
        {
            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class DoStuffAction : HypermediaAction
            {
                public DoStuffAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Item A", Classes = ["ItemA"])]
            public class HypermediaItemHto : HypermediaObject
            {
                public string Value { get; set; } = string.Empty;

                [HypermediaAction(Name = "DoStuff")]
                public DoStuffAction? DoStuff { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class ItemController : ControllerBase
            {
                [HttpPost("do")]
                [HypermediaActionEndpoint<NsA.HypermediaItemHto>("DoStuff",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult Do() => Ok();
            }
        }

        namespace NsB
        {
            // [HypermediaSchemaName] disambiguates — both HTOs would derive "Item" (RY0024)
            [HypermediaObject(Title = "Item B", Classes = ["ItemB"])]
            [RESTyard.Schema.Model.HypermediaSchemaName("ItemB")]
            public class HypermediaItemHto : HypermediaObject
            {
                public string Value { get; set; } = string.Empty;

                [HypermediaAction(Name = "DoStuff")]
                public NsA.DoStuffAction? DoStuff { get; set; }
            }
        }
        """;

    [Fact]
    public void Same_class_name_in_two_namespaces_generates_both()
    {
        // GEN-05: hint names were keyed on the bare class name — the second HTO
        // crashed the whole generation with a duplicate-hint-name ArgumentException.
        var result = GeneratorTestHelper.RunGenerator(SameClassNameInTwoNamespacesSource);

        result.Diagnostics.Should().BeEmpty();
        result.GeneratedTrees.Should().ContainSingle(t => t.FilePath.Contains("NsA.HypermediaItemHtoSchema.g.cs"));
        result.GeneratedTrees.Should().ContainSingle(t => t.FilePath.Contains("NsB.HypermediaItemHtoSchema.g.cs"));

        GeneratorTestHelper.AssertOutputCompiles(SameClassNameInTwoNamespacesSource);
    }

    [Fact]
    public void ResultType_does_not_leak_to_same_named_hto_in_other_namespace()
    {
        // GEN-05: result mappings were keyed on the bare class name — the HTO in NsB
        // received the ResultType declared for the one in NsA.
        var result = GeneratorTestHelper.RunGenerator(SameClassNameInTwoNamespacesSource);

        GetGeneratedSource(result, "NsA.HypermediaItemHto").Should().Contain("ResultName");
        GetGeneratedSource(result, "NsB.HypermediaItemHto").Should().NotContain("ResultName");
    }

    // --- Generated code robustness (GEN-07) ---

    [Fact]
    public void Title_with_control_characters_round_trips()
    {
        // GEN-07: EscapeString only handled backslash and quote — a literal newline
        // in a title broke the emitted string literal.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Line1\nLine2\tEnd", Classes = ["Truck"])]
            public class HypermediaTruckHto : HypermediaObject
            {
                public string Value { get; set; } = string.Empty;
            }
            """;

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaTruckHto", source);

        schema.Title.Should().Be("Line1\nLine2\tEnd");
    }

    [Fact]
    public void Forwarded_attribute_arguments_use_invariant_literals_with_type_suffixes()
    {
        // GEN-07: FormatTypedConstant fell through to Value.ToString() — culture-sensitive
        // for floating point ("1,5" on a de-DE machine), no suffixes (a float/long attribute
        // constructor would not re-resolve), chars emitted unquoted.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class NumericAttribute : System.Attribute
            {
                public NumericAttribute(double d, float f, long l, char c) { }
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                [Numeric(1.5, 2.5f, 5L, 'x')]
                public string Value { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var poco = GetGeneratedPoco(result, "HypermediaCustomerHto");

        poco.Should().Contain("1.5d");
        poco.Should().Contain("2.5f");
        poco.Should().Contain("5L");
        poco.Should().Contain("'x'");

        GeneratorTestHelper.AssertOutputCompiles(source);
    }

    [Fact]
    public void Keyword_property_name_override_is_escaped()
    {
        // GEN-07: a [HypermediaProperty(Name = "class")] override is applied structurally
        // as the POCO property name — keywords must be @-escaped to compile.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                [HypermediaProperty(Name = "class")]
                public string Kind { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();
        GetGeneratedPoco(result, "HypermediaCustomerHto").Should().Contain("public string @class");

        GeneratorTestHelper.AssertOutputCompiles(source);
    }

    [Fact]
    public void Invalid_property_name_override_reports_RY0022_and_falls_back()
    {
        // GEN-07: "full-name" cannot be a C# property name — report it and use the
        // original property name instead of emitting code that does not compile.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                [HypermediaProperty(Name = "full-name")]
                public string FullName { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Where(d => d.Id == "RY0022").Should().ContainSingle()
            .Which.GetMessage().Should().Contain("full-name");
        GetGeneratedPoco(result, "HypermediaCustomerHto").Should().Contain("public string FullName");

        GeneratorTestHelper.AssertOutputCompiles(source);
    }

    [Fact]
    public void User_defined_Properties_type_reports_RY0023_and_skips_poco()
    {
        // GEN-07: a user type named {ClassName}Properties collided with the generated
        // POCO as an unexplained CS0101 — report the real cause instead.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class HypermediaCustomerHtoProperties
            {
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Where(d => d.Id == "RY0023").Should().ContainSingle()
            .Which.GetMessage().Should().Contain("HypermediaCustomerHtoProperties");
        result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("Properties.g.cs"));

        GeneratorTestHelper.AssertOutputCompiles(source);
    }

    [Fact]
    public void User_defined_SirenHelper_type_reports_RY0023_and_skips_helper()
    {
        // GEN-07: the shared SirenHelper is emitted in the global namespace — a
        // user-defined global SirenHelper would collide.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly(Siren = true)]

            public class SirenHelper
            {
            }

            namespace TestHtos
            {
                [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
                public class HypermediaCustomerHto : HypermediaObject
                {
                    public string Name { get; set; } = string.Empty;
                }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Where(d => d.Id == "RY0023").Should().ContainSingle()
            .Which.GetMessage().Should().Contain("SirenHelper");
        result.GeneratedTrees.Should().NotContain(t => t.FilePath.EndsWith("SirenHelper.g.cs"));
    }

    [Fact]
    public void Named_argument_201_response_emits_RY0031_warning()
    {
        // GEN-15: [ProducesResponseType(StatusCode = 201)] / [SwaggerResponse(StatusCode = 201)]
        // set the status via a named argument — previously only constructor arguments matched.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public class SwaggerResponseAttribute : System.Attribute
            {
                public int StatusCode { get; set; }
            }

            public class SomeAction : HypermediaAction
            {
                public SomeAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "Create")]
                public SomeAction? Create { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPost("create")]
                [SwaggerResponse(StatusCode = 201)]
                [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
                public IActionResult Create() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().Contain(d => d.Id == "RY0031");
    }

    [Fact]
    public void Duplicate_embedded_entity_relations_emit_RY0041_info()
    {
        // GEN-15: identical [Relations] on two embedded entity properties is valid Siren
        // (and allowed at runtime), so this is only an Info-level hint — unlike RY0040 for links.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Address", Classes = ["Address"])]
            public class HypermediaAddressHto : HypermediaObject
            {
                public string Street { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                [Relations(["address"])]
                public IEmbeddedEntity<HypermediaAddressHto>? HomeAddress { get; set; }

                [Relations(["address"])]
                public IEmbeddedEntity<HypermediaAddressHto>? WorkAddress { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var ry0041 = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0041").Which;
        ry0041.Severity.Should().Be(DiagnosticSeverity.Info);
        ry0041.GetMessage().Should().Contain("HomeAddress").And.Contain("WorkAddress");
        ry0041.Location.Should().NotBe(Location.None);
        result.Diagnostics.Should().NotContain(d => d.Id == "RY0040");
    }

    [Fact]
    public void Inheritdoc_on_override_property_resolves_base_doc_in_poco()
    {
        // GEN-15: a verbatim <inheritdoc/> resolves to nothing on the generated POCO
        // (the POCO property overrides nothing) — resolve it to the base property's doc.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            public abstract class CustomerBase : HypermediaObject
            {
                /// <summary>The customer's display name.</summary>
                public virtual string Name { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : CustomerBase
            {
                /// <inheritdoc/>
                public override string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var poco = GetGeneratedPoco(result, "HypermediaCustomerHto");
        poco.Should().Contain("The customer's display name.");
        poco.Should().NotContain("<inheritdoc");
    }

    [Fact]
    public void Diagnostic_location_points_at_offending_property()
    {
        // GEN-13: diagnostics carry the property's source location (file offset)
        // instead of Location.None so the IDE can navigate to it.
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Other", Classes = ["Other"])]
            public class HypermediaOtherHto : HypermediaObject { }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public ILink<HypermediaOtherHto>? MissingRelLink { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var diagnostic = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0021").Which;
        var span = diagnostic.Location.SourceSpan;
        source.Substring(span.Start, span.Length).Should().Be("MissingRelLink");
    }

    // --- Legacy attribute existence check ---
    // If this test fails, the legacy HttpMethodHypermediaAction was removed.
    // Remove ActionResultMappingExtractor.ExtractLegacyActionResults (and its InheritsFrom
    // check), and delete this test.
    [Fact]
    public void Legacy_HttpMethodHypermediaAction_type_exists()
    {
        typeof(RESTyard.AspNetCore.WebApi.AttributedRoutes.HttpMethodHypermediaAction)
            .Should().NotBeNull();
    }

    // --- Step 2.9.1: Schema registry generation ---

    [Fact]
    public void FullHto_generates_registry_with_assembly_attribute()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);
        var registry = GetGeneratedRegistry(result);

        registry.Should().Contain("HypermediaSchemaRegistry_TestAssembly");
        registry.Should().Contain("[assembly: global::RESTyard.Schema.Model.HypermediaSchemaRegistryAttribute(typeof(HypermediaSchemaRegistry_TestAssembly))]");
    }

    [Fact]
    public void FullHto_registry_calls_GetSchema_for_each_hto()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);
        var registry = GetGeneratedRegistry(result);

        // Customer has properties → needs schemaFactory
        registry.Should().Contain("HypermediaCustomerHtoSchema.GetSchema(schemaFactory)");
        // Address has properties → needs schemaFactory
        registry.Should().Contain("HypermediaAddressHtoSchema.GetSchema(schemaFactory)");
    }

    [Fact]
    public void Registry_uses_parameterless_GetSchema_for_hto_without_properties()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Empty", Classes = ["Empty"])]
            public class HypermediaEmptyHto : HypermediaObject
            {
            }

            [HypermediaObject(Title = "Other", Classes = ["Other"])]
            public class HypermediaOtherHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        var registry = GetGeneratedRegistry(result);

        // Empty has no properties → parameterless
        registry.Should().Contain("HypermediaEmptyHtoSchema.GetSchema()");
        // Other has properties → with factory
        registry.Should().Contain("HypermediaOtherHtoSchema.GetSchema(schemaFactory)");
    }

    [Fact]
    public void FullHto_registry_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.FullHto);
    }

    [Fact]
    public void SimpleHto_registry_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.SimpleHto);
    }

    [Fact]
    public void Source_without_HypermediaAssembly_has_no_registry()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("Registry"));
    }

    // --- Step 2.9: [HypermediaAssembly] opt-in gating ---

    [Fact]
    public void Source_without_HypermediaAssembly_attribute_generates_nothing()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.GeneratedTrees.Should().BeEmpty();
        result.Diagnostics.Should().BeEmpty();
    }

    [Fact]
    public void Source_with_Schema_false_generates_nothing()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly(Schema = false)]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.GeneratedTrees.Should().BeEmpty();
    }

    [Fact]
    public void Source_with_Siren_true_and_Schema_false_emits_warning_and_generates_schema()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly(Siren = true, Schema = false)]

            namespace TestHtos;

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        // Schema should be forced to true — output generated
        result.GeneratedTrees.Should().NotBeEmpty();

        // GEN-12: a warning (error + forcing Schema = true would be contradictory),
        // GEN-13: pointing at the [assembly: HypermediaAssembly] attribute.
        var ry0030 = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0030").Which;
        ry0030.Severity.Should().Be(DiagnosticSeverity.Warning);
        ry0030.Location.Should().NotBe(Location.None);
    }

    // --- Step 2.8: Deprecation support ---

    [Fact]
    public void HtoWithDeprecation_entity_is_deprecated()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithDeprecation);

        schema.IsDeprecated.Should().BeTrue();
        schema.DeprecationMessage.Should().Be("Use HypermediaCustomerV2Hto instead.");
    }

    [Fact]
    public void HtoWithDeprecation_link_is_deprecated()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithDeprecation);

        var link = schema.Links.Single(l => l.Relations.Contains("bestFriend"));
        link.IsDeprecated.Should().BeTrue();
        link.DeprecationMessage.Should().Be("Use preferredFriend instead.");
    }

    [Fact]
    public void HtoWithDeprecation_action_is_deprecated_without_message()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithDeprecation);

        var action = schema.Actions.Single(a => a.Name == "MarkAsFavorite");
        action.IsDeprecated.Should().BeTrue();
        action.DeprecationMessage.Should().BeNull();
    }

    [Fact]
    public void HtoWithDeprecation_embedded_entity_is_deprecated()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithDeprecation);

        var embedded = schema.EmbeddedEntities.Single(e => e.Relations.Contains("address"));
        embedded.IsDeprecated.Should().BeTrue();
        embedded.DeprecationMessage.Should().Be("Use primaryAddress instead.");
    }

    [Fact]
    public void HtoWithDeprecation_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithDeprecation);
    }

    [Fact]
    public void SimpleHto_is_not_deprecated()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.SimpleHto);

        schema.IsDeprecated.Should().BeFalse();
        schema.DeprecationMessage.Should().BeNull();
    }

    // --- Step 2.7.1a: Properties POCO generation ---

    [Fact]
    public void SimpleHto_generates_properties_poco()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);
        var poco = GetGeneratedPoco(result, "HypermediaCustomerHto");

        poco.Should().Contain("public class HypermediaCustomerHtoProperties");
        poco.Should().Contain("public string Name { get; set; }");
        poco.Should().Contain("public int Age { get; set; }");
    }

    [Fact]
    public void SimpleHto_properties_poco_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.SimpleHto);
    }

    [Fact]
    public void Hto_without_properties_does_not_generate_poco()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Empty", Classes = ["Empty"])]
            public class HypermediaEmptyHto : HypermediaObject
            {
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.GeneratedTrees.Should().NotContain(t => t.FilePath.Contains("Properties.g.cs"));
    }

    [Fact]
    public void MixedAttributes_poco_forwards_third_party_attributes()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        // [JsonPropertyName] should be forwarded (full attribute class name with Attribute suffix)
        poco.Should().Contain("JsonPropertyNameAttribute(\"display_name\")");

        // [JsonConverter] should be forwarded
        poco.Should().Contain("JsonConverterAttribute(typeof(global::TestHtos.MyCustomConverter))");
    }

    [Fact]
    public void MixedAttributes_poco_excludes_restyard_attributes()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        // RESTyard attributes should NOT appear
        poco.Should().NotContain("HypermediaProperty");
        poco.Should().NotContain("FormatterIgnore");
        poco.Should().NotContain("KeyAttribute");
        poco.Should().NotContain("Relations");
        poco.Should().NotContain("HypermediaAction");
    }

    [Fact]
    public void MixedAttributes_poco_applies_HypermediaProperty_name_structurally()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        // [HypermediaProperty(Name = "DisplayName")] should rename the property
        poco.Should().Contain("public string DisplayName { get; set; }");
        // Original C# name "Name" should not appear as a property
        poco.Should().NotMatchRegex(@"public string Name\b");
    }

    [Fact]
    public void MixedAttributes_poco_excludes_FormatterIgnore_and_Key_properties()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        // [FormatterIgnoreHypermediaProperty] properties excluded
        poco.Should().NotContain("InternalCode");

        // Links and actions excluded
        poco.Should().NotContain("Self");
        poco.Should().NotContain("Mark");
    }

    [Fact]
    public void MixedAttributes_poco_includes_Key_property_without_Key_attribute()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        // [Key] properties are data properties that also serve as route keys.
        // They appear in Siren output and the POCO, but the [Key] attribute itself
        // is RESTyard-specific and should not be forwarded.
        poco.Should().Contain("public int Id { get; set; }");
        poco.Should().NotContain("KeyAttribute");
        poco.Should().NotContain("RouteResolver");
    }

    [Fact]
    public void MixedAttributes_poco_copies_xml_doc_comments()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithMixedAttributes);
        var poco = GetGeneratedPoco(result, "HypermediaProductHto");

        poco.Should().Contain("/// <summary>The product display name.</summary>");
        poco.Should().Contain("/// <summary>The product price in USD.</summary>");
    }

    [Fact]
    public void MixedAttributes_poco_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithMixedAttributes);
    }

    [Fact]
    public void FullHto_generates_poco_for_each_hto_with_properties()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);

        result.GeneratedTrees.Should().Contain(t => t.FilePath.Contains("HypermediaCustomerHtoProperties.g.cs"));
        result.GeneratedTrees.Should().Contain(t => t.FilePath.Contains("HypermediaAddressHtoProperties.g.cs"));
    }

    [Fact]
    public void FullHto_poco_excludes_links_actions_embedded()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.FullHto);
        var poco = GetGeneratedPoco(result, "HypermediaCustomerHto");

        poco.Should().Contain("public string Name { get; set; }");
        poco.Should().Contain("public int Age { get; set; }");
        poco.Should().NotContain("Self");
        poco.Should().NotContain("BestFriend");
        poco.Should().NotContain("MarkAsFavorite");
        poco.Should().NotContain("BuyCar");
        poco.Should().NotContain("Address");
    }

    // --- Schema name override and collisions (GEN-06) ---

    private const string SchemaNameOverrideSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.Schema.Model;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        [HypermediaSchemaName("CrmCustomer")]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
        }

        [HypermediaObject(Title = "Root", Classes = ["Root"])]
        public class HypermediaRootHto : HypermediaObject
        {
            [Relations(["customer"])]
            public ILink<HypermediaCustomerHto>? Customer { get; set; }
        }
        """;

    [Fact]
    public void SchemaName_attribute_overrides_derived_name_and_link_targets()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", SchemaNameOverrideSource);
        schema.Name.Should().Be("CrmCustomer");

        // Cross-references must use the override too, or they dangle
        var rootSchema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaRootHto", SchemaNameOverrideSource);
        rootSchema.Links.Should().ContainSingle()
            .Which.TargetName.Should().Be("CrmCustomer");
    }

    [Fact]
    public void SchemaName_attribute_applies_to_action_result_names()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using RESTyard.Schema.Model;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "QueryResult", Classes = ["QueryResult"])]
            [HypermediaSchemaName("SearchResult")]
            public class HypermediaQueryResultHto : HypermediaObject
            {
                public string ResultData { get; set; } = string.Empty;
            }

            public class DoStuffAction : HypermediaAction
            {
                public DoStuffAction() : base(() => true) { }
            }

            [HypermediaObject(Title = "Item", Classes = ["Item"])]
            public class HypermediaItemHto : HypermediaObject
            {
                [HypermediaAction(Name = "DoStuff")]
                public DoStuffAction? DoStuff { get; set; }
            }

            [ApiController]
            [Route("api")]
            public class ItemController : ControllerBase
            {
                [HttpPost("do")]
                [HypermediaActionEndpoint<HypermediaItemHto>("DoStuff",
                    ResultType = typeof(HypermediaQueryResultHto))]
                public IActionResult Do() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();
        GetGeneratedSource(result, "HypermediaItemHto")
            .Should().Contain("ResultName = \"SearchResult\"");
    }

    private const string DuplicateSchemaNamesSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        [HypermediaObject(Title = "Customer A", Classes = ["CustomerA"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
        }

        [HypermediaObject(Title = "Customer B", Classes = ["CustomerB"])]
        public class CustomerHto : HypermediaObject
        {
            public string OtherName { get; set; } = string.Empty;
        }
        """;

    [Fact]
    public void Duplicate_schema_names_report_RY0024_error()
    {
        // GEN-06: HypermediaCustomerHto and CustomerHto both derive "Customer" —
        // cross-references would silently point at the wrong entity.
        var result = GeneratorTestHelper.RunGenerator(DuplicateSchemaNamesSource);

        var diagnostic = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0024").Which;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("TestHtos.CustomerHto")
            .And.Contain("TestHtos.HypermediaCustomerHto")
            .And.Contain("'Customer'");
    }

    [Fact]
    public void Duplicate_schema_names_resolved_by_attribute_are_silent()
    {
        var source = DuplicateSchemaNamesSource.Replace(
            "[HypermediaObject(Title = \"Customer B\", Classes = [\"CustomerB\"])]",
            """
            [HypermediaObject(Title = "Customer B", Classes = ["CustomerB"])]
            [RESTyard.Schema.Model.HypermediaSchemaName("LegacyCustomer")]
            """);

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().BeEmpty();
    }

    // --- record HTOs (GEN-08) ---

    private const string RecordHtoSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public record HypermediaCustomerHto(string Name, int Age) : IHypermediaObject
        {
            public string? Nickname { get; init; }
        }
        """;

    [Fact]
    public void Record_hto_generates_schema_and_poco()
    {
        // GEN-08: the syntax predicate matched only ClassDeclarationSyntax —
        // record HTOs produced no schema and no mapper, silently.
        var result = GeneratorTestHelper.RunGenerator(RecordHtoSource);

        result.Diagnostics.Should().BeEmpty();

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", RecordHtoSource);
        schema.Name.Should().Be("Customer");

        // Positional (primary-constructor) and body properties are data properties;
        // the compiler-generated EqualityContract is protected and must not leak in.
        var poco = result.GeneratedTrees
            .Single(t => t.FilePath.Contains("HypermediaCustomerHtoProperties.g.cs"))
            .GetText().ToString();
        poco.Should().Contain("Name")
            .And.Contain("Age")
            .And.Contain("Nickname")
            .And.NotContain("EqualityContract");

        GeneratorTestHelper.AssertOutputCompiles(RecordHtoSource);
    }

    // --- Embedded collection detection (GEN-09) ---

    private const string ArrayEmbeddedSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Attributes;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        [HypermediaObject(Title = "Order", Classes = ["Order"])]
        public class HypermediaOrderHto : HypermediaObject
        {
            public string OrderNumber { get; set; } = string.Empty;
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Relations(["orders"])]
            public IEmbeddedEntity<HypermediaOrderHto>[] Orders { get; set; } = [];
        }
        """;

    [Fact]
    public void Array_of_embedded_entities_is_a_collection_embedded_entity()
    {
        // GEN-09: arrays were not detected as embedded collections and leaked
        // into the data-properties POCO as a data property.
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", ArrayEmbeddedSource);

        var embedded = schema.EmbeddedEntities.Should().ContainSingle().Which;
        embedded.IsCollection.Should().BeTrue();
        embedded.TargetName.Should().Be("Order");

        var result = GeneratorTestHelper.RunGenerator(ArrayEmbeddedSource);
        result.GeneratedTrees
            .Single(t => t.FilePath.Contains("HypermediaCustomerHtoProperties.g.cs"))
            .GetText().ToString()
            .Should().NotContain("Orders");
    }

    [Fact]
    public void Array_of_embedded_entities_without_relations_reports_RY0020()
    {
        var source = ArrayEmbeddedSource.Replace("[Relations([\"orders\"])]", string.Empty);

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0020");
    }

    [Fact]
    public void Non_collection_generics_over_embedded_entities_are_not_embedded()
    {
        // GEN-09: any generic type whose first type argument was an embedded entity counted
        // as a collection — Func<IEmbeddedEntity<T>> and Dictionary<IEmbeddedEntity<T>, X>
        // were classified as embedded (and reported RY0020 here, before [FormatterIgnore...]
        // could exclude them). Only IEnumerable<IEmbeddedEntity<T>> implementers qualify.
        const string source = """
            using System;
            using System.Collections.Generic;
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Attributes;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Order", Classes = ["Order"])]
            public class HypermediaOrderHto : HypermediaObject
            {
                public string OrderNumber { get; set; } = string.Empty;
            }

            [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
            public class HypermediaCustomerHto : HypermediaObject
            {
                public string Name { get; set; } = string.Empty;

                [FormatterIgnoreHypermediaProperty]
                public Func<IEmbeddedEntity<HypermediaOrderHto>>? Factory { get; set; }

                [FormatterIgnoreHypermediaProperty]
                public Dictionary<IEmbeddedEntity<HypermediaOrderHto>, string>? Lookup { get; set; }
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);
        result.Diagnostics.Should().BeEmpty();

        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema("HypermediaCustomerHto", source);
        schema.EmbeddedEntities.Should().BeEmpty();
    }

    private static string GetGeneratedSource(
        GeneratorDriverRunResult result,
        string htoClassName)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains($"{htoClassName}Schema.g.cs"));

        tree.Should().NotBeNull($"expected generated source for {htoClassName}");
        return tree!.GetText().ToString();
    }

    private static string GetGeneratedSirenSource(
        GeneratorDriverRunResult result,
        string htoClassName)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains($"{htoClassName}SirenExtensions.g.cs"));

        tree.Should().NotBeNull($"expected generated Siren source for {htoClassName}");
        return tree!.GetText().ToString();
    }

    private static string GetGeneratedPoco(
        GeneratorDriverRunResult result,
        string htoClassName)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains($"{htoClassName}Properties.g.cs"));

        tree.Should().NotBeNull($"expected generated POCO for {htoClassName}");
        return tree!.GetText().ToString();
    }

    private static string GetGeneratedRegistry(GeneratorDriverRunResult result)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains("HypermediaSchemaRegistry.g.cs"));

        tree.Should().NotBeNull("expected generated schema registry");
        return tree!.GetText().ToString();
    }

    // --- Access Groups tests ---

    [Fact]
    public void HtoWithAccessGroups_entity_has_AccessGroups()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAccessGroups);

        schema.AccessGroups.Should().NotBeNull();
        schema.AccessGroups.Should().BeEquivalentTo(["admin", "sales"]);
    }

    [Fact]
    public void HtoWithAccessGroups_action_has_AccessGroups()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAccessGroups);

        var deleteAction = schema.Actions.Single(a => a.Name == "DeleteCustomer");
        deleteAction.AccessGroups.Should().BeEquivalentTo(["admin"]);
    }

    [Fact]
    public void HtoWithAccessGroups_link_has_AccessGroups()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAccessGroups);

        var ordersLink = schema.Links.Single(l => l.Relations.Contains("orders"));
        ordersLink.AccessGroups.Should().BeEquivalentTo(["read"]);
    }

    [Fact]
    public void HtoWithAccessGroups_link_without_access_group_has_null()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAccessGroups);

        var selfLink = schema.Links.Single(l => l.Relations.Contains("self"));
        selfLink.AccessGroups.Should().BeNull();
    }

    [Fact]
    public void HtoWithAccessGroups_embedded_has_AccessGroups()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaCustomerHto", TestHtoSources.HtoWithAccessGroups);

        var addressEmbedded = schema.EmbeddedEntities.Single(e => e.Relations.Contains("address"));
        addressEmbedded.AccessGroups.Should().BeEquivalentTo(["read", "write"]);
    }

    [Fact]
    public void HtoWithoutAccessGroups_has_null_AccessGroups()
    {
        var schema = GeneratorTestHelper.RunGeneratorAndGetSchema(
            "HypermediaSimpleHto", TestHtoSources.HtoWithoutAccessGroups);

        schema.AccessGroups.Should().BeNull();
        schema.Links.Single().AccessGroups.Should().BeNull();
    }

    [Fact]
    public void HtoWithAccessGroups_compiles_correctly()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithAccessGroups);
    }

    // --- Siren (ToSiren / ToSirenEmbedded) tests ---

    [Fact]
    public void SimpleHtoWithSiren_generates_ToSiren_and_ToSirenEmbedded()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        result.Diagnostics.Should().BeEmpty();

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("public sealed class HypermediaCustomerHtoSiren : SirenEntity<HypermediaCustomerHtoProperties>");
        sirenSource.Should().Contain("public sealed class HypermediaCustomerHtoSirenEmbedded : SirenEmbeddedEntity<HypermediaCustomerHtoProperties>");
        sirenSource.Should().Contain("public static HypermediaCustomerHtoSiren ToSiren(");
        sirenSource.Should().Contain("public static HypermediaCustomerHtoSirenEmbedded ToSirenEmbedded(");
        sirenSource.Should().Contain("IHypermediaRouteResolver resolver");
        sirenSource.Should().Contain("IQueryStringBuilder queryStringBuilder");
        sirenSource.Should().Contain("SirenMapperOptions? options = null");
    }

    [Fact]
    public void SimpleHtoWithSiren_ToSiren_maps_class_and_title()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("Class = new[] { \"Customer\" }");
        sirenSource.Should().Contain("Title = \"Customer\"");
    }

    [Fact]
    public void SimpleHtoWithSiren_ToSiren_maps_properties_to_poco()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("Properties = new HypermediaCustomerHtoProperties");
        sirenSource.Should().Contain("Name = hto.Name,");
        sirenSource.Should().Contain("Age = hto.Age,");
    }

    [Fact]
    public void SimpleHtoWithSiren_ToSiren_adds_self_link()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("resolver.ObjectToRoute(hto)");
        sirenSource.Should().Contain("effectiveOptions.AutoSelfLink");
        sirenSource.Should().Contain("Rel = new[] { \"self\" }");
        sirenSource.Should().Contain("Href = selfRoute.Url");
    }

    [Fact]
    public void HtoWithExplicitSelfLink_suppresses_auto_self_link()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithLinksWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        // Should NOT contain auto self link — HTO has explicit [Relations(["self"])]
        sirenSource.Should().NotContain("effectiveOptions.AutoSelfLink");
        sirenSource.Should().NotContain("Rel = new[] { \"self\" }, Href = selfRoute.Url");
        // But should still contain the explicit self link via SirenHelper.AddLink
        sirenSource.Should().Contain("SirenHelper.AddLink(entity.Links, hto.Self,");
    }

    [Fact]
    public void SimpleHtoWithSiren_ToSirenEmbedded_has_EditorBrowsable_Never()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("[System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]");
    }

    [Fact]
    public void SimpleHtoWithSiren_generated_source_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.SimpleHtoWithSiren);
    }

    [Fact]
    public void HtoWithoutClasses_WithSiren_falls_back_to_type_name()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithoutClassesWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaWidgetHto");
        sirenSource.Should().Contain("Class = new[] { \"HypermediaWidgetHto\" }");
    }

    [Fact]
    public void SimpleHto_without_Siren_flag_does_not_emit_ToSiren()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHto);

        var sirenFiles = result.GeneratedTrees
            .Where(t => t.FilePath.Contains("SirenExtensions.g.cs"))
            .ToArray();

        sirenFiles.Should().BeEmpty("Siren = false (default) should not emit ToSiren methods");
    }

    [Fact]
    public void SimpleHtoWithSiren_emits_separate_schema_and_siren_classes()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.SimpleHtoWithSiren);

        var schemaSource = GetGeneratedSource(result, "HypermediaCustomerHto");
        schemaSource.Should().Contain("public static class HypermediaCustomerHtoSchema");

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("public static class HypermediaCustomerHtoSirenExtensions");
    }

    [Fact]
    public void EmptyHtoWithSiren_uses_NoProperties()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.EmptyHtoWithSiren);

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaEmptyHto");
        sirenSource.Should().Contain("HypermediaEmptyHtoSiren : SirenEntity<NoProperties>");
        sirenSource.Should().Contain("HypermediaEmptyHtoSirenEmbedded : SirenEmbeddedEntity<NoProperties>");
        sirenSource.Should().NotContain("Properties =");
    }

    [Fact]
    public void EmptyHtoWithSiren_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.EmptyHtoWithSiren);
    }

    // --- Step 6.2: Link resolution tests ---

    [Fact]
    public void HtoWithLinksWithSiren_generates_link_resolution_code()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithLinksWithSiren);

        result.Diagnostics.Should().BeEmpty();

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        // Mandatory link — direct call to SirenHelper
        sirenSource.Should().Contain("SirenHelper.AddLink(entity.Links, hto.Self,");
        // Optional link — null-checked then call to SirenHelper
        sirenSource.Should().Contain("if (hto.BestFriend is { } BestFriendLink)");
        sirenSource.Should().Contain("SirenHelper.AddLink(entity.Links, BestFriendLink,");
    }

    [Fact]
    public void HtoWithLinksWithSiren_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithLinksWithSiren);
    }

    // --- Step 6.3: Action resolution tests ---

    [Fact]
    public void HtoWithActionsWithSiren_generates_action_resolution_code()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithActionsWithSiren);

        result.Diagnostics.Should().BeEmpty();

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        // Parameterless action — CanExecute null-safe check
        sirenSource.Should().Contain("hto.MarkAsFavorite?.CanExecute() == true");
        sirenSource.Should().Contain("SirenHelper.AddAction(entity.Actions, hto, hto.MarkAsFavorite");
        sirenSource.Should().Contain("\"MarkAsFavorite\"");
        // Parameterized action with user-defined classes
        sirenSource.Should().Contain("hto.CustomerMove?.CanExecute() == true");
        sirenSource.Should().Contain("\"CustomerMove\"");
        sirenSource.Should().Contain("\"Destructive\"");
    }

    [Fact]
    public void HtoWithActionsWithSiren_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithActionsWithSiren);
    }

    [Fact]
    public void Mandatory_action_generates_null_guard_that_throws()
    {
        var result = GeneratorTestHelper.RunGenerator(HtoWithMandatoryAction);

        result.Diagnostics.Should().BeEmpty();

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        sirenSource.Should().Contain("if (hto.MarkAsFavorite is null)");
        sirenSource.Should().Contain(
            "Mandatory action 'MarkAsFavorite' on 'HypermediaCustomerHto' is null");
        // Unavailable mandatory action is a contract violation, not a silent omission
        sirenSource.Should().Contain("if (!hto.MarkAsFavorite.CanExecute())");
        sirenSource.Should().Contain(
            "Mandatory action 'MarkAsFavorite' on 'HypermediaCustomerHto' is not available");
        sirenSource.Should().NotContain("MarkAsFavorite?.CanExecute()");
    }

    [Fact]
    public void Mandatory_unavailable_action_throws_at_render_time()
    {
        var resolver = new StubRouteResolver(new ResolvedRoute("http://test/self", "GET"));

        var act = () => GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options: null,
            HtoWithDisabledMandatoryAction);

        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*Disable*HypermediaCustomerHto*not available*");
    }

    [Fact]
    public void Mandatory_null_action_throws_at_render_time()
    {
        var resolver = new StubRouteResolver(new ResolvedRoute("http://test/self", "GET"));

        var act = () => GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options: null, HtoWithMandatoryAction);

        // Reflection invoke wraps the InvalidOperationException from the generated mapper
        act.Should().Throw<System.Reflection.TargetInvocationException>()
            .WithInnerException<InvalidOperationException>()
            .WithMessage("*MarkAsFavorite*HypermediaCustomerHto*");
    }

    [Fact]
    public void Nullable_null_action_is_silently_omitted()
    {
        var resolver = new StubRouteResolver(new ResolvedRoute("http://test/self", "GET"));

        // Both actions in the fixture are nullable and left null
        var json = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto", resolver, configureHto: null, options: null,
            TestHtoSources.HtoWithActionsWithSiren);

        using var doc = JsonDocument.Parse(json);
        var hasActions = doc.RootElement.TryGetProperty("actions", out var actions);
        (!hasActions || actions.GetArrayLength() == 0).Should().BeTrue(
            "null nullable actions must not be rendered");
    }

    private const string HtoWithMandatoryAction = """
        using System;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.Schema.Model;

        [assembly: HypermediaAssembly(Siren = true)]

        namespace TestHtos;

        public class MarkAsFavoriteOp : HypermediaAction
        {
            public MarkAsFavoriteOp() : base(() => true) { }
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            [HypermediaAction(Name = "MarkAsFavorite", Title = "Mark as Favorite")]
            public MarkAsFavoriteOp MarkAsFavorite { get; set; } = default!;
        }
        """;

    private const string HtoWithDisabledMandatoryAction = """
        using System;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.Schema.Model;

        [assembly: HypermediaAssembly(Siren = true)]

        namespace TestHtos;

        public class DisableOp : HypermediaAction
        {
            public DisableOp() : base(() => false) { }
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            [HypermediaAction(Name = "Disable", Title = "Never available")]
            public DisableOp Disable { get; set; } = new DisableOp();
        }
        """;

    // --- Step 6.4: Embedded entity resolution tests ---

    [Fact]
    public void HtoWithEmbeddedWithSiren_generates_embedded_entity_resolution_code()
    {
        var result = GeneratorTestHelper.RunGenerator(TestHtoSources.HtoWithEmbeddedWithSiren);

        result.Diagnostics.Should().BeEmpty();

        var sirenSource = GetGeneratedSirenSource(result, "HypermediaCustomerHto");
        // Nullable single — skip when null
        sirenSource.Should().Contain("hto.Address is { } AddressValue");
        sirenSource.Should().Contain("reference.IsResolved()");
        sirenSource.Should().Contain("ToSirenEmbedded(resolver, queryStringBuilder, options)");
        sirenSource.Should().Contain("new[] { \"address\" }");
        // Mandatory collection — null guard
        sirenSource.Should().Contain("hto.Addresses is null");
        sirenSource.Should().Contain("InvalidOperationException");
        sirenSource.Should().Contain("foreach (var item in hto.Addresses)");
        // Unresolved path — linked sub-entity via resolver
        sirenSource.Should().Contain("SirenLinkedEntity");
        sirenSource.Should().Contain("resolver.ReferenceToRoute(reference)");
        // HypermediaExternalObjectReference is NOT handled (dead code in SirenConverter — ctor throws)
        sirenSource.Should().NotContain("HypermediaExternalObjectReference");
    }

    [Fact]
    public void HtoWithEmbeddedWithSiren_compiles()
    {
        GeneratorTestHelper.AssertOutputCompiles(TestHtoSources.HtoWithEmbeddedWithSiren);
    }

    // --- Parity tests: ToSiren() vs SirenConverter ---

    [Fact]
    public void SimpleHtoWithSiren_ToSiren_parity_with_SirenConverter()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/customers/42", "GET"));

        // Generated ToSiren()
        var generatedJson = GeneratorTestHelper.RunGeneratorAndGetSirenJson(
            "HypermediaCustomerHto",
            resolver,
            hto =>
            {
                hto.GetType().GetProperty("Name")!.SetValue(hto, "John");
                hto.GetType().GetProperty("Age")!.SetValue(hto, 30);
            },
            options: null,
            TestHtoSources.SimpleHtoWithSiren);

        // Reflection-based SirenConverter
        var htoAssembly = GeneratorTestHelper.EmitAssembly(TestHtoSources.SimpleHtoWithSiren);
        var htoType = htoAssembly.GetType("TestHtos.HypermediaCustomerHto")!;
        var htoInstance = (IHypermediaObject)Activator.CreateInstance(htoType)!;
        htoType.GetProperty("Name")!.SetValue(htoInstance, "John");
        htoType.GetProperty("Age")!.SetValue(htoInstance, 30);

        var converter = new RESTyard.AspNetCore.WebApi.Formatter.SirenConverter(
            resolver, new RESTyard.AspNetCore.Query.QueryStringBuilder());
        var converterJson = converter.ConvertToString(htoInstance);

        // Compare normalized JSON — focus on class, title, properties, self link
        var normalizedGenerated = GeneratorTestHelper.NormalizeJson(generatedJson);
        var normalizedConverter = GeneratorTestHelper.NormalizeJson(converterJson);

        // Parse both for structural comparison
        using var genDoc = JsonDocument.Parse(normalizedGenerated);
        using var convDoc = JsonDocument.Parse(normalizedConverter);

        var genRoot = genDoc.RootElement;
        var convRoot = convDoc.RootElement;

        // Class
        genRoot.GetProperty("class").ToString().Should().Be(convRoot.GetProperty("class").ToString());

        // Title
        genRoot.GetProperty("title").GetString().Should().Be(convRoot.GetProperty("title").GetString());

        // Properties
        genRoot.GetProperty("properties").ToString().Should().Be(convRoot.GetProperty("properties").ToString());

        // Self link — SirenConverter adds "self" link automatically from ObjectToRoute
        // Note: SirenConverter uses lowercase "rel", "href" — verify both outputs have self links
        // Self link parity deferred to Step 6.2 — SirenConverter resolves links from ILink properties,
        // while ToSiren() uses resolver.ObjectToRoute(). Full link parity requires link resolution (Step 6.2).
    }

    // --- Duplicate endpoint diagnostics (GEN-11, RY0033) ---

    private const string DuplicateEndpointBaseSource = """
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.AspNetCore.WebApi.AttributedRoutes;
        using Microsoft.AspNetCore.Mvc;

        [assembly: HypermediaAssembly]

        namespace TestHtos;

        public class CreateOp : HypermediaAction
        {
            public CreateOp() : base(() => true) { }
        }

        [HypermediaObject(Title = "Root", Classes = ["Root"])]
        public class HypermediaRootHto : HypermediaObject
        {
            [HypermediaAction(Name = "Create")]
            public CreateOp? Create { get; set; }
        }

        [ApiController]
        [Route("api")]
        public class RootController : ControllerBase
        {
            [HttpGet("root")]
            [HypermediaObjectEndpoint<HypermediaRootHto>]
            public IActionResult GetRoot() => Ok();

            [HttpPost("create")]
            [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
            public IActionResult Create() => Ok();
        }
        """;

    [Fact]
    public void Single_endpoint_per_hto_and_action_is_silent()
    {
        var result = GeneratorTestHelper.RunGenerator(DuplicateEndpointBaseSource);

        result.Diagnostics.Should().NotContain(d => d.Id == "RY0033");
    }

    [Fact]
    public void Duplicate_action_endpoints_report_RY0033_error()
    {
        var source = DuplicateEndpointBaseSource.Replace(
            "public IActionResult Create() => Ok();",
            """
            public IActionResult Create() => Ok();

                [HttpPost("create2")]
                [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
                public IActionResult CreateAgain() => Ok();
            """);

        var result = GeneratorTestHelper.RunGenerator(source);

        var diagnostic = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0033").Which;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("HypermediaRootHto.Create");
        // Reported on the surplus (second) attribute in file order
        diagnostic.Location.Should().NotBe(Location.None);
        source.Substring(diagnostic.Location.SourceSpan.Start)
            .Should().StartWith("HypermediaActionEndpoint<HypermediaRootHto>(\"Create\")");
        diagnostic.Location.SourceSpan.Start.Should().BeGreaterThan(
            source.IndexOf("create2", StringComparison.Ordinal));
    }

    [Fact]
    public void Duplicate_object_endpoints_report_RY0033_error()
    {
        var source = DuplicateEndpointBaseSource.Replace(
            "public IActionResult GetRoot() => Ok();",
            """
            public IActionResult GetRoot() => Ok();

                [HttpGet("root2")]
                [HypermediaObjectEndpoint<HypermediaRootHto>]
                public IActionResult GetRootAgain() => Ok();
            """);

        var result = GeneratorTestHelper.RunGenerator(source);

        var diagnostic = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0033").Which;
        diagnostic.Severity.Should().Be(DiagnosticSeverity.Error);
        diagnostic.GetMessage().Should().Contain("'HypermediaRootHto'");
    }

    [Fact]
    public void Triplicate_action_endpoints_report_one_RY0033_per_surplus_endpoint()
    {
        var source = DuplicateEndpointBaseSource.Replace(
            "public IActionResult Create() => Ok();",
            """
            public IActionResult Create() => Ok();

                [HttpPost("create2")]
                [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
                public IActionResult CreateAgain() => Ok();

                [HttpPost("create3")]
                [HypermediaActionEndpoint<HypermediaRootHto>("Create")]
                public IActionResult CreateOnceMore() => Ok();
            """);

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Where(d => d.Id == "RY0033").Should().HaveCount(2);
    }

    [Fact]
    public void Endpoints_for_different_actions_are_silent()
    {
        var source = DuplicateEndpointBaseSource
            .Replace(
                "public CreateOp? Create { get; set; }",
                """
                public CreateOp? Create { get; set; }

                    [HypermediaAction(Name = "Update")]
                    public CreateOp? Update { get; set; }
                """)
            .Replace(
                "public IActionResult Create() => Ok();",
                """
                public IActionResult Create() => Ok();

                    [HttpPost("update")]
                    [HypermediaActionEndpoint<HypermediaRootHto>("Update")]
                    public IActionResult Update() => Ok();
                """);

        var result = GeneratorTestHelper.RunGenerator(source);

        result.Diagnostics.Should().NotContain(d => d.Id == "RY0033");
    }

    [Fact]
    public void Duplicate_legacy_action_endpoints_report_RY0033_error()
    {
        const string source = """
            using RESTyard.AspNetCore.Hypermedia;
            using RESTyard.AspNetCore.Hypermedia.Actions;
            using RESTyard.AspNetCore.Hypermedia.Attributes;
            using RESTyard.AspNetCore.WebApi.AttributedRoutes;
            using Microsoft.AspNetCore.Mvc;

            [assembly: HypermediaAssembly]

            namespace TestHtos;

            [HypermediaObject(Title = "Root", Classes = ["Root"])]
            public class HypermediaRootHto : HypermediaObject
            {
                [HypermediaAction(Name = "Create")]
                public CreateOp? Create { get; set; }

                public class CreateOp : HypermediaAction
                {
                    public CreateOp() : base(() => true) { }
                }
            }

            [ApiController]
            [Route("api")]
            public class RootController : ControllerBase
            {
                [HttpPostHypermediaAction("create", typeof(HypermediaRootHto.CreateOp))]
                public IActionResult Create() => Ok();

                [HttpPostHypermediaAction("create2", typeof(HypermediaRootHto.CreateOp))]
                public IActionResult CreateAgain() => Ok();
            }
            """;

        var result = GeneratorTestHelper.RunGenerator(source);

        var diagnostic = result.Diagnostics.Should().ContainSingle(d => d.Id == "RY0033").Which;
        diagnostic.GetMessage().Should().Contain("HypermediaRootHto.Create");
    }
}
