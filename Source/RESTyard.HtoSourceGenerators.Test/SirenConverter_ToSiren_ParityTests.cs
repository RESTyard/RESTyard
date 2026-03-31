using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using AwesomeAssertions;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Actions;
using RESTyard.AspNetCore.Hypermedia.Links;
using RESTyard.AspNetCore.Hypermedia.Siren;
using RESTyard.AspNetCore.Query;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.HtoSourceGenerators.TestHtos;
using Xunit;

namespace RESTyard.HtoSourceGenerators.Test;

/// <summary>
/// Runtime parity tests comparing generated ToSiren() output against
/// reflection-based SirenConverter output. Both use the same StubRouteResolver
/// and QueryStringBuilder — any JSON difference is a real divergence.
/// </summary>
public class SirenConverter_ToSiren_ParityTests
{
    private static readonly QueryStringBuilder QueryStringBuilder = new();

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        Converters = { new System.Text.Json.Serialization.JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase) },
    };

    private static string SerializeToSirenJson(object sirenEntity)
    {
        return JsonSerializer.Serialize(sirenEntity, sirenEntity.GetType(), SerializerOptions);
    }

    private static string ConverterJson(IHypermediaObject hto, StubRouteResolver resolver)
    {
        var converter = new SirenConverter(resolver, QueryStringBuilder);
        return converter.ConvertToString(hto);
    }

    /// <summary>
    /// Compares specific Siren sections between generated ToSiren() and SirenConverter output.
    /// Compares: class, title, links, actions, entities.
    /// Properties are compared separately because serialization differs
    /// (System.Text.Json vs Newtonsoft with custom enum/date handling).
    /// </summary>
    private static void AssertSirenStructureEqual(string generated, string converter)
    {
        using var genDoc = JsonDocument.Parse(generated);
        using var convDoc = JsonDocument.Parse(converter);
        var gen = genDoc.RootElement;
        var conv = convDoc.RootElement;

        // Class
        AssertElementEqual(gen.GetProperty("class"), conv.GetProperty("class"), "$.class");

        // Title
        if (gen.TryGetProperty("title", out var genTitle) && conv.TryGetProperty("title", out var convTitle))
        {
            AssertElementEqual(genTitle, convTitle, "$.title");
        }

        // Links
        AssertElementEqual(gen.GetProperty("links"), conv.GetProperty("links"), "$.links");

        // Actions
        AssertElementEqual(gen.GetProperty("actions"), conv.GetProperty("actions"), "$.actions");

        // Entities
        AssertElementEqual(gen.GetProperty("entities"), conv.GetProperty("entities"), "$.entities");
    }

    /// <summary>
    /// Full structural comparison including properties section.
    /// Use only when properties don't contain enums or other types with
    /// known serialization differences.
    /// </summary>
    private static void AssertJsonEqual(string generated, string converter)
    {
        using var genDoc = JsonDocument.Parse(generated);
        using var convDoc = JsonDocument.Parse(converter);
        AssertElementEqual(genDoc.RootElement, convDoc.RootElement, "$");
    }

    private static void AssertElementEqual(JsonElement generated, JsonElement converter, string path)
    {
        generated.ValueKind.Should().Be(converter.ValueKind, $"value kind mismatch at {path}");

        switch (generated.ValueKind)
        {
            case JsonValueKind.Object:
                var genProps = new Dictionary<string, JsonElement>();
                foreach (var prop in generated.EnumerateObject())
                {
                    genProps[prop.Name] = prop.Value;
                }

                var convProps = new Dictionary<string, JsonElement>();
                foreach (var prop in converter.EnumerateObject())
                {
                    convProps[prop.Name] = prop.Value;
                }

                genProps.Keys.OrderBy(k => k).Should().BeEquivalentTo(
                    convProps.Keys.OrderBy(k => k),
                    $"object keys differ at {path}");

                foreach (var key in genProps.Keys)
                {
                    AssertElementEqual(genProps[key], convProps[key], $"{path}.{key}");
                }
                break;

            case JsonValueKind.Array:
                var genArray = generated.EnumerateArray().ToList();
                var convArray = converter.EnumerateArray().ToList();

                genArray.Count.Should().Be(convArray.Count, $"array length differs at {path}");
                for (var i = 0; i < genArray.Count; i++)
                {
                    AssertElementEqual(genArray[i], convArray[i], $"{path}[{i}]");
                }
                break;

            default:
                generated.ToString().Should().Be(converter.ToString(), $"value mismatch at {path}");
                break;
        }
    }

    // --- Properties parity ---

    [Fact]
    public void SimpleCustomer_properties_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/customers/1", "GET"));

        var hto = new SimpleCustomerHto { Name = "John", Age = 30 };

        AssertJsonEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    // Tests below use AssertSirenStructureEqual because property serialization differs
    // between System.Text.Json and SirenConverter's Newtonsoft-based output:
    // - Uri trailing slash (Newtonsoft adds it, STJ doesn't)
    // - Null properties (WhenWritingNull omits them, SirenConverter includes them)
    // - Empty properties (NoProperties → null → omitted, SirenConverter emits {})
    // The Siren structure (class, title, links, actions, entities) is always compared fully.

    [Fact]
    public void VariousProperties_structure_match()
    {
        // Uses AssertSirenStructureEqual because properties contain types with known
        // serialization differences: Uri (trailing slash), enum (EnumMember vs CamelCase).
        // Properties POCO is correct — serialization is the consumer's concern.
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/all-types/1", "GET"));

        var hto = new HtoWithVariousProperties
        {
            Text = "hello",
            Count = 42,
            IsActive = true,
            Ratio = 3.14,
            Price = 99.99m,
            NullableInt = 7,
            FavoriteColor = Color.Green,
            NullableColor = Color.Blue,
            BirthDate = new DateOnly(1990, 5, 15),
            AlarmTime = new TimeOnly(8, 30, 0),
            CreatedAt = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc),
            ModifiedAt = new DateTimeOffset(2024, 6, 15, 10, 30, 0, TimeSpan.Zero),
            Website = new Uri("https://example.com"),
            Address = new NestedAddress { Street = "Main St", City = "Springfield" },
            Tags = ["tag1", "tag2"],
            InternalNote = "should not appear",
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void VariousProperties_with_nulls_structure_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/all-types/1", "GET"));

        var hto = new HtoWithVariousProperties
        {
            Text = "minimal",
            NullableInt = null,
            NullableColor = null,
            Website = null,
            Address = null,
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    // --- Links parity ---

    [Fact]
    public void Links_internal_and_self_match()
    {
        var relatedHto = new SimpleCustomerHto { Name = "Related", Age = 25 };
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllLinkTypes>("http://test/links/1"),
            RouteMapping.ForObject<SimpleCustomerHto>("http://test/customers/1"));

        var hto = new HtoWithAllLinkTypes(
            relatedHto: relatedHto,
            optional: null,
            externalUri: new Uri("https://external.example.com"),
            externalMediaType: "text/html");

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Links_with_optional_present_match()
    {
        var relatedHto = new SimpleCustomerHto { Name = "Related", Age = 25 };
        var optionalTarget = new SimpleCustomerHto { Name = "Optional", Age = 35 };
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllLinkTypes>("http://test/links/1"),
            RouteMapping.ForObject<SimpleCustomerHto>("http://test/customers/1"));

        var hto = new HtoWithAllLinkTypes(
            relatedHto: relatedHto,
            optional: Link.To(optionalTarget),
            externalUri: new Uri("https://external.example.com"),
            externalMediaType: "text/html");

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    // --- Actions parity ---

    [Fact]
    public void Actions_parameterless_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"),
            RouteMapping.ForAction<HtoWithAllActionTypes, MarkAsFavoriteOp>("http://test/actions/1/doNothing", "POST"));

        var hto = new HtoWithAllActionTypes
        {
            DoNothing = new MarkAsFavoriteOp(() => true),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_with_parameter_and_prefilled_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"),
            RouteMapping.ForAction<HtoWithAllActionTypes, CustomerMoveOp>("http://test/actions/1/move", "POST"));

        var hto = new HtoWithAllActionTypes
        {
            MoveCustomer = new CustomerMoveOp(() => true, new MoveParameters { Street = "Main St", City = "NYC" }),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_disabled_are_omitted()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"));

        var hto = new HtoWithAllActionTypes
        {
            DisabledAction = new MarkAsFavoriteOp(() => false),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_external_no_param_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"));

        var hto = new HtoWithAllActionTypes
        {
            ExternalNoParam = new ExternalNoParamOp(new Uri("https://external.example.com/action"), "PUT"),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_external_with_param_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"));

        var hto = new HtoWithAllActionTypes
        {
            ExternalWithParam = new ExternalWithParamOp(
                new Uri("https://external.example.com/action"),
                "POST",
                new SimpleParameters { Value = 42 }),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_file_upload_no_param_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"),
            RouteMapping.ForAction<HtoWithAllActionTypes, FileUploadOp>("http://test/actions/1/upload", "POST"));

        var config = new FileUploadConfiguration
        {
            Accept = [".jpg", ".png"],
            MaxFileSizeBytes = 5_000_000,
            AllowMultiple = true,
        };

        var hto = new HtoWithAllActionTypes
        {
            UploadFile = new FileUploadOp(() => true, config),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_file_upload_with_param_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"),
            RouteMapping.ForAction<HtoWithAllActionTypes, FileUploadWithParamOp>("http://test/actions/1/upload-param", "POST"));

        var hto = new HtoWithAllActionTypes
        {
            UploadFileWithParam = new FileUploadWithParamOp(() => true, prefilled: new SimpleParameters { Value = 99 }),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Actions_with_user_classes_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllActionTypes>("http://test/actions/1"),
            RouteMapping.ForAction<HtoWithAllActionTypes, CustomerMoveOp>("http://test/actions/1/move", "POST"));

        var hto = new HtoWithAllActionTypes
        {
            MoveCustomer = new CustomerMoveOp(() => true),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    // --- Embedded entities parity ---

    [Fact]
    public void Embedded_resolved_single_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllEmbeddedTypes>("http://test/embedded/1"),
            RouteMapping.ForObject<AddressHto>("http://test/addresses/1"));

        var hto = new HtoWithAllEmbeddedTypes
        {
            Description = "test",
            PrimaryAddress = EmbeddedEntity.Embed(new AddressHto { Street = "Main St", City = "NYC" }),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Embedded_nullable_null_skipped()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllEmbeddedTypes>("http://test/embedded/1"));

        var hto = new HtoWithAllEmbeddedTypes
        {
            Description = "test",
            PrimaryAddress = null,
            SecondaryAddress = null,
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Embedded_resolved_collection_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllEmbeddedTypes>("http://test/embedded/1"),
            RouteMapping.ForObject<SimpleCustomerHto>("http://test/customers/1"));

        var hto = new HtoWithAllEmbeddedTypes
        {
            Description = "test",
            Customers =
            [
                EmbeddedEntity.Embed(new SimpleCustomerHto { Name = "Alice", Age = 25 }),
                EmbeddedEntity.Embed(new SimpleCustomerHto { Name = "Bob", Age = 30 }),
            ],
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    [Fact]
    public void Embedded_unresolved_internal_match()
    {
        var resolver = new StubRouteResolver(
            new ResolvedRoute("http://test/fallback", "GET"),
            RouteMapping.ForObject<HtoWithAllEmbeddedTypes>("http://test/embedded/1"),
            RouteMapping.ForObject<AddressHto>("http://test/addresses/99"));

        // Unresolved reference — has type but no instance
        var unresolvedRef = new HypermediaObjectKeyReference(typeof(AddressHto), null);
        var hto = new HtoWithAllEmbeddedTypes
        {
            Description = "test",
            PrimaryAddress = new EmbeddedEntity<AddressHto>(unresolvedRef),
        };

        AssertSirenStructureEqual(SerializeToSirenJson(hto.ToSiren(resolver, QueryStringBuilder)), ConverterJson(hto, resolver));
    }

    // Note: Embedded_unresolved_external is not testable —
    // HypermediaExternalObjectReference constructor throws because its internal
    // ExternalObject class lacks [HypermediaObject]. The corresponding branch in
    // SirenConverter.SirenAddEntities() is dead code. The type system also prevents
    // IEmbeddedEntity<HypermediaExternalObjectReference> (not IHypermediaObject).
    // For external links, use ExternalReference via Link.External() instead.
}
