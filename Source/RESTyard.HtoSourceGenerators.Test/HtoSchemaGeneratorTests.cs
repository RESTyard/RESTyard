using System.Linq;
using System.Text.Json;
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
            "HypermediaAddressHtoProperties.g.cs",
            "HypermediaAddressHtoSirenMapper.g.cs",
            "HypermediaCustomerHtoProperties.g.cs",
            "HypermediaCustomerHtoSirenMapper.g.cs",
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
        result.Diagnostics.Should().Contain(d => d.GetMessage().Contains("MissingRelations"));
        result.Diagnostics.Should().Contain(d => d.GetMessage().Contains("AlsoMissing"));
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
        registry.Should().Contain("HypermediaCustomerHtoSirenMapper.GetSchema(schemaFactory)");
        // Address has properties → needs schemaFactory
        registry.Should().Contain("HypermediaAddressHtoSirenMapper.GetSchema(schemaFactory)");
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
        registry.Should().Contain("HypermediaEmptyHtoSirenMapper.GetSchema()");
        // Other has properties → with factory
        registry.Should().Contain("HypermediaOtherHtoSirenMapper.GetSchema(schemaFactory)");
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

        // Warning emitted
        result.Diagnostics.Should().Contain(d => d.Id == "RY0030");
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

    private static string GetGeneratedSource(
        GeneratorDriverRunResult result,
        string htoClassName)
    {
        var tree = result.GeneratedTrees
            .SingleOrDefault(t => t.FilePath.Contains($"{htoClassName}SirenMapper"));

        tree.Should().NotBeNull($"expected generated source for {htoClassName}");
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
}
