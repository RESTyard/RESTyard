using System.Text.Json;
using AwesomeAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RESTyard.AspNetCore.Hypermedia.Siren.Model;

namespace RESTyard.AspNetCore.Test.Hypermedia.Siren.Model;

[TestClass]
public class SirenModelSerializationTests
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = false,
    };

    [TestMethod]
    public void SirenEntity_RoundTrip_PreservesAllProperties()
    {
        var entity = new SirenEntity<object>
        {
            Class = ["Customer", "Entity"],
            Title = "A Customer",
            Links = [new SirenLink { Rel = ["self"], Href = "/customers/42" }],
            Actions =
            [
                new SirenAction
                {
                    Name = "delete",
                    Href = "/customers/42",
                    Method = "DELETE",
                    Title = "Delete Customer",
                },
            ],
            Entities =
            [
                new SirenLinkedEntity
                {
                    Rel = ["order"],
                    Href = "/orders/100",
                    Class = ["Order"],
                    Title = "Order 100",
                },
            ],
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);

        // Use JsonDocument for verification since SirenSubEntity deserialization
        // requires a custom converter (structural discrimination)
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        root.GetProperty("class").GetArrayLength().Should().Be(2);
        root.GetProperty("title").GetString().Should().Be("A Customer");
        root.GetProperty("links").GetArrayLength().Should().Be(1);
        root.GetProperty("links")[0].GetProperty("href").GetString().Should().Be("/customers/42");
        root.GetProperty("actions").GetArrayLength().Should().Be(1);
        root.GetProperty("actions")[0].GetProperty("name").GetString().Should().Be("delete");
        root.GetProperty("actions")[0].GetProperty("method").GetString().Should().Be("DELETE");
        root.GetProperty("entities").GetArrayLength().Should().Be(1);
        root.GetProperty("entities")[0].GetProperty("href").GetString().Should().Be("/orders/100");
    }

    [TestMethod]
    public void SirenEntityGeneric_RoundTrip_PreservesTypedProperties()
    {
        var entity = new SirenEntity<TestProperties>
        {
            Class = ["Customer"],
            Title = "Customer 42",
            Properties = new TestProperties { Name = "John", Age = 30 },
            Links = [new SirenLink { Rel = ["self"], Href = "/customers/42" }],
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<SirenEntity<TestProperties>>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Properties.Should().NotBeNull();
        deserialized.Properties!.Name.Should().Be("John");
        deserialized.Properties.Age.Should().Be(30);
        deserialized.Class.Should().BeEquivalentTo(["Customer"]);
    }

    [TestMethod]
    public void SirenLink_RoundTrip_PreservesAllProperties()
    {
        var link = new SirenLink
        {
            Rel = ["self", "canonical"],
            Href = "/customers/42",
            Title = "Customer 42",
            Type = "application/vnd.siren+json",
            Class = ["primary"],
        };

        var json = JsonSerializer.Serialize(link, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<SirenLink>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Rel.Should().BeEquivalentTo(["self", "canonical"]);
        deserialized.Href.Should().Be("/customers/42");
        deserialized.Title.Should().Be("Customer 42");
        deserialized.Type.Should().Be("application/vnd.siren+json");
        deserialized.Class.Should().BeEquivalentTo(["primary"]);
    }

    [TestMethod]
    public void SirenAction_RoundTrip_PreservesAllProperties()
    {
        var action = new SirenAction
        {
            Name = "create-customer",
            Href = "/customers",
            Class = ["create"],
            Title = "Create Customer",
            Method = "POST",
            Type = "application/json",
            Fields =
            [
                new SirenField
                {
                    Name = "customerData",
                    Class = ["http://example.com/schemas/customer"],
                    Type = "application/json",
                    Title = "Customer Data",
                },
            ],
        };

        var json = JsonSerializer.Serialize(action, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<SirenAction>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Name.Should().Be("create-customer");
        deserialized.Href.Should().Be("/customers");
        deserialized.Method.Should().Be("POST");
        deserialized.Type.Should().Be("application/json");
        deserialized.Fields.Should().HaveCount(1);
        deserialized.Fields![0].Name.Should().Be("customerData");
        deserialized.Fields[0].Type.Should().Be("application/json");
    }

    [TestMethod]
    public void SirenField_FileUploadExtensions_RoundTrip()
    {
        var field = new SirenField
        {
            Name = "file",
            Type = "file",
            Accept = ".pdf,.jpg",
            MaxFileSizeBytes = 10_000_000,
            AllowMultiple = true,
        };

        var json = JsonSerializer.Serialize(field, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<SirenField>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Accept.Should().Be(".pdf,.jpg");
        deserialized.MaxFileSizeBytes.Should().Be(10_000_000);
        deserialized.AllowMultiple.Should().Be(true);
    }

    [TestMethod]
    public void SirenField_FileUploadExtensions_OmittedWhenNull()
    {
        var field = new SirenField { Name = "data", Type = "application/json" };

        var json = JsonSerializer.Serialize(field, JsonOptions);

        json.Should().NotContain("accept");
        json.Should().NotContain("maxFileSizeBytes");
        json.Should().NotContain("allowMultiple");
    }

    [TestMethod]
    public void SirenEmbeddedEntity_Serialization_IncludesDerivedProperties()
    {
        var embedded = new SirenEmbeddedEntity<object>
        {
            Rel = ["item"],
            Class = ["Order"],
            Title = "Order 100",
            Properties = new { OrderNumber = 100, Status = "shipped" },
            Links = [new SirenLink { Rel = ["self"], Href = "/orders/100" }],
        };

        // Serialize as base type to verify [JsonDerivedType] includes derived properties
        var json = JsonSerializer.Serialize<SirenSubEntity>(embedded, JsonOptions);
        json.Should().Contain("\"rel\"");
        json.Should().Contain("\"class\"");
        json.Should().Contain("\"properties\"");
        json.Should().Contain("\"links\"");

        // Round-trip works when deserializing as concrete type
        var deserialized = JsonSerializer.Deserialize<SirenEmbeddedEntity<object>>(json, JsonOptions);
        deserialized.Should().NotBeNull();
        deserialized!.Class.Should().BeEquivalentTo(["Order"]);
        deserialized.Title.Should().Be("Order 100");
    }

    [TestMethod]
    public void SirenEmbeddedEntityGeneric_TypeSafe_SerializesCorrectly()
    {
        var embedded = new SirenEmbeddedEntity<TestProperties>
        {
            Rel = ["item"],
            Class = ["Customer"],
            Properties = new TestProperties { Name = "John", Age = 30 },
            Links = [new SirenLink { Rel = ["self"], Href = "/customers/42" }],
        };

        // Type safety: compiler enforces correct property type
        embedded.Properties!.Name.Should().Be("John");

        // Serialize as SirenSubEntity (the list element type) — generic properties must survive
        var json = JsonSerializer.Serialize<SirenSubEntity>(embedded, JsonOptions);

        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("properties").GetProperty("name").GetString().Should().Be("John");
        root.GetProperty("properties").GetProperty("age").GetInt32().Should().Be(30);
        root.GetProperty("class")[0].GetString().Should().Be("Customer");
    }

    [TestMethod]
    public void SirenLinkedEntity_Serialization_IncludesDerivedProperties()
    {
        var linked = new SirenLinkedEntity
        {
            Rel = ["order"],
            Href = "/orders/100",
            Class = ["Order"],
            Title = "Order 100",
            Type = "application/vnd.siren+json",
        };

        // Serialize as base type to verify [JsonDerivedType] includes derived properties
        var json = JsonSerializer.Serialize<SirenSubEntity>(linked, JsonOptions);

        json.Should().Contain("\"href\":\"/orders/100\"");
        json.Should().Contain("\"rel\"");
        json.Should().Contain("\"class\"");
        json.Should().Contain("\"title\"");
        json.Should().Contain("\"type\"");

        // Round-trip works when deserializing as concrete type
        var deserialized = JsonSerializer.Deserialize<SirenLinkedEntity>(json, JsonOptions);
        deserialized.Should().NotBeNull();
        deserialized!.Href.Should().Be("/orders/100");
        deserialized.Class.Should().BeEquivalentTo(["Order"]);
        deserialized.Title.Should().Be("Order 100");
        deserialized.Type.Should().Be("application/vnd.siren+json");
    }

    [TestMethod]
    public void SirenEntity_JsonPropertyNames_UseSirenConventions()
    {
        var entity = new SirenEntity<object>
        {
            Class = ["Test"],
            Title = "Test",
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);

        json.Should().Contain("\"class\"");
        json.Should().Contain("\"title\"");
        json.Should().NotContain("\"Class\"");
        json.Should().NotContain("\"Title\"");
    }

    [TestMethod]
    public void SirenEntity_NullCollections_OmittedFromJson()
    {
        var entity = new SirenEntity<object>
        {
            Class = ["Test"],
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);

        json.Should().NotContain("\"links\"");
        json.Should().NotContain("\"actions\"");
        json.Should().NotContain("\"entities\"");
        json.Should().NotContain("\"title\"");
    }

    [TestMethod]
    public void FullSirenDocument_RoundTrip()
    {
        var entity = new SirenEntity<TestProperties>
        {
            Class = ["Customer"],
            Title = "Customer 42",
            Properties = new TestProperties { Name = "John", Age = 30 },
            Links =
            [
                new SirenLink { Rel = ["self"], Href = "/customers/42" },
                new SirenLink { Rel = ["collection"], Href = "/customers" },
            ],
            Actions =
            [
                new SirenAction
                {
                    Name = "move",
                    Href = "/customers/42/move",
                    Method = "POST",
                    Type = "application/json",
                    Fields = [new SirenField { Name = "newAddress", Type = "application/json" }],
                },
            ],
            Entities =
            [
                new SirenEmbeddedEntity<object>
                {
                    Rel = ["item"],
                    Class = ["Order"],
                    Properties = new { OrderNumber = 1 },
                    Links = [new SirenLink { Rel = ["self"], Href = "/orders/1" }],
                },
                new SirenLinkedEntity
                {
                    Rel = ["related"],
                    Href = "/products/5",
                    Class = ["Product"],
                },
            ],
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);

        // Verify JSON structure contains all expected sections
        json.Should().Contain("\"class\"");
        json.Should().Contain("\"properties\"");
        json.Should().Contain("\"links\"");
        json.Should().Contain("\"actions\"");
        json.Should().Contain("\"entities\"");
        json.Should().Contain("\"Customer\"");
        json.Should().Contain("\"John\"");
        json.Should().Contain("/customers/42");
        json.Should().Contain("\"move\"");
        json.Should().Contain("\"Order\"");
        json.Should().Contain("\"Product\"");

        // Verify round-trip for the parts that don't require polymorphic deserialization
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        root.GetProperty("class")[0].GetString().Should().Be("Customer");
        root.GetProperty("title").GetString().Should().Be("Customer 42");
        root.GetProperty("properties").GetProperty("name").GetString().Should().Be("John");
        root.GetProperty("links").GetArrayLength().Should().Be(2);
        root.GetProperty("actions").GetArrayLength().Should().Be(1);
        root.GetProperty("entities").GetArrayLength().Should().Be(2);
    }

    [TestMethod]
    public void SirenSubEntityConverter_DeserializesEmbeddedEntity_WhenPropertiesPresent()
    {
        var json = """
            {
                "rel": ["item"],
                "class": ["Order"],
                "title": "Order 100",
                "properties": { "orderNumber": 100, "status": "shipped" },
                "links": [{ "rel": ["self"], "href": "/orders/100" }]
            }
            """;

        var result = JsonSerializer.Deserialize<SirenSubEntity>(json, JsonOptions);

        result.Should().BeOfType<SirenEmbeddedEntity<JsonElement>>();
        var embedded = (SirenEmbeddedEntity<JsonElement>)result!;
        embedded.Rel.Should().BeEquivalentTo(["item"]);
        embedded.Class.Should().BeEquivalentTo(["Order"]);
        embedded.Title.Should().Be("Order 100");
        embedded.Properties.GetProperty("orderNumber").GetInt32().Should().Be(100);
        embedded.Links.Should().HaveCount(1);
    }

    [TestMethod]
    public void SirenSubEntityConverter_DeserializesEmbeddedEntity_WhenEntitiesPresent()
    {
        var json = """
            {
                "rel": ["parent"],
                "class": ["Container"],
                "entities": [
                    { "rel": ["child"], "href": "/items/1" }
                ]
            }
            """;

        var result = JsonSerializer.Deserialize<SirenSubEntity>(json, JsonOptions);

        result.Should().BeOfType<SirenEmbeddedEntity<JsonElement>>();
        var embedded = (SirenEmbeddedEntity<JsonElement>)result!;
        embedded.Rel.Should().BeEquivalentTo(["parent"]);
        embedded.Entities.Should().HaveCount(1);
    }

    [TestMethod]
    public void SirenSubEntityConverter_DeserializesLinkedEntity_WhenHrefPresent()
    {
        var json = """
            {
                "rel": ["order"],
                "href": "/orders/100",
                "class": ["Order"],
                "title": "Order 100",
                "type": "application/vnd.siren+json"
            }
            """;

        var result = JsonSerializer.Deserialize<SirenSubEntity>(json, JsonOptions);

        result.Should().BeOfType<SirenLinkedEntity>();
        var linked = (SirenLinkedEntity)result!;
        linked.Rel.Should().BeEquivalentTo(["order"]);
        linked.Href.Should().Be("/orders/100");
        linked.Class.Should().BeEquivalentTo(["Order"]);
        linked.Title.Should().Be("Order 100");
        linked.Type.Should().Be("application/vnd.siren+json");
    }

    [TestMethod]
    public void SirenSubEntityConverter_ThrowsOnAmbiguousSubEntity()
    {
        var json = """{ "rel": ["unknown"] }""";

        var act = () => JsonSerializer.Deserialize<SirenSubEntity>(json, JsonOptions);

        act.Should().Throw<JsonException>();
    }

    [TestMethod]
    public void SirenSubEntityConverter_FullRoundTrip_WithMixedSubEntities()
    {
        var entity = new SirenEntity<TestProperties>
        {
            Class = ["Customer"],
            Properties = new TestProperties { Name = "John", Age = 30 },
            Entities =
            [
                new SirenEmbeddedEntity<object>
                {
                    Rel = ["item"],
                    Class = ["Order"],
                    Properties = new { OrderNumber = 1 },
                },
                new SirenLinkedEntity
                {
                    Rel = ["related"],
                    Href = "/products/5",
                    Class = ["Product"],
                },
            ],
        };

        var json = JsonSerializer.Serialize(entity, JsonOptions);
        var deserialized = JsonSerializer.Deserialize<SirenEntity<TestProperties>>(json, JsonOptions);

        deserialized.Should().NotBeNull();
        deserialized!.Entities.Should().HaveCount(2);
        deserialized.Entities![0].Should().BeOfType<SirenEmbeddedEntity<JsonElement>>();
        deserialized.Entities[1].Should().BeOfType<SirenLinkedEntity>();

        var embedded = (SirenEmbeddedEntity<JsonElement>)deserialized.Entities[0];
        embedded.Class.Should().BeEquivalentTo(["Order"]);

        var linked = (SirenLinkedEntity)deserialized.Entities[1];
        linked.Href.Should().Be("/products/5");
    }

    private class TestProperties
    {
        public string? Name { get; set; }
        public int Age { get; set; }
    }
}
