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

        // Links should not appear in propertySchemas (they go to Links array)
        source.Should().NotContain("propertySchemas[\"Self\"]");
        source.Should().NotContain("propertySchemas[\"BestFriend\"]");

        // Actions should not appear in propertySchemas
        source.Should().NotContain("propertySchemas[\"MarkAsFavorite\"]");
        source.Should().NotContain("propertySchemas[\"BuyCar\"]");

        // Embedded entities should not appear in propertySchemas
        source.Should().NotContain("propertySchemas[\"Address\"]");
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
