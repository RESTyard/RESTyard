namespace RESTyard.HtoSourceGenerators.Test;

/// <summary>
/// Reusable HTO source strings for generator tests.
/// Each source is a complete compilable unit with the necessary using statements.
/// </summary>
internal static class TestHtoSources
{
    private const string Usings = """
        using System;
        using System.Collections.Generic;
        using Json.Schema.Generation;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        """;

    private const string AssemblyAttribute = """

        [assembly: HypermediaAssembly]
        """;

    private const string AssemblyAttributeWithSiren = """

        [assembly: HypermediaAssembly(Siren = true)]
        """;

    /// <summary>
    /// Minimal HTO with one string property.
    /// </summary>
    internal const string SimpleHto = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }
        """;

    /// <summary>
    /// Minimal HTO with Siren = true — generates ToSiren() and ToSirenEmbedded() in addition to GetSchema().
    /// </summary>
    internal const string SimpleHtoWithSiren = $$"""
        {{Usings}}
        {{AssemblyAttributeWithSiren}}

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }
        """;

    /// <summary>
    /// HTO with no data properties — tests SirenNoProperties usage.
    /// </summary>
    internal const string EmptyHtoWithSiren = $$"""
        {{Usings}}
        {{AssemblyAttributeWithSiren}}

        namespace TestHtos;

        [HypermediaObject(Title = "Empty", Classes = ["Empty"])]
        public class HypermediaEmptyHto : HypermediaObject
        {
        }
        """;

    /// <summary>
    /// HTO with no explicit Classes — tests fallback to type name.
    /// </summary>
    internal const string HtoWithoutClassesWithSiren = $$"""
        {{Usings}}
        {{AssemblyAttributeWithSiren}}

        namespace TestHtos;

        [HypermediaObject(Title = "A Widget")]
        public class HypermediaWidgetHto : HypermediaObject
        {
            public string Label { get; set; } = string.Empty;
        }
        """;

    /// <summary>
    /// HTO with mandatory and optional links.
    /// </summary>
    internal const string HtoWithLinks = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Relations(["self"])]
            public ILink<HypermediaCustomerHto> Self { get; set; } = default!;

            [Relations(["bestFriend"])]
            public ILink<HypermediaCustomerHto>? BestFriend { get; set; }
        }
        """;

    /// <summary>
    /// HTO with parameterless and parameterized actions.
    /// </summary>
    internal const string HtoWithActions = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        public class MarkAsFavoriteAction : HypermediaAction
        {
            public MarkAsFavoriteAction() : base(() => true) { }
        }

        public class BuyCarParameters : IHypermediaActionParameter
        {
            public string CarId { get; set; } = string.Empty;
        }

        public class BuyCarAction : HypermediaAction<BuyCarParameters>
        {
            public BuyCarAction() : base() { }
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [HypermediaAction]
            public MarkAsFavoriteAction? MarkAsFavorite { get; set; }

            [HypermediaAction]
            public BuyCarAction? BuyCar { get; set; }
        }
        """;

    /// <summary>
    /// HTO with single and collection embedded entities.
    /// </summary>
    internal const string HtoWithEmbedded = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

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

            [Relations(["address"])]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }

            [Relations(["addresses"])]
            public List<IEmbeddedEntity<HypermediaAddressHto>> Addresses { get; set; } = new();
        }
        """;

    /// <summary>
    /// HTO with various primitive and well-known property types.
    /// </summary>
    internal const string HtoWithVariousPropertyTypes = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "AllTypes", Classes = ["AllTypes"])]
        public class HypermediaAllTypesHto : HypermediaObject
        {
            public string Text { get; set; } = string.Empty;
            public bool IsActive { get; set; }
            public int Count { get; set; }
            public long BigNumber { get; set; }
            public double Ratio { get; set; }
            public decimal Price { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTimeOffset ModifiedAt { get; set; }
            public DateOnly BirthDate { get; set; }
            public TimeOnly AlarmTime { get; set; }
            public TimeSpan Duration { get; set; }
            public Uri Website { get; set; } = default!;
            public Guid ExternalId { get; set; }
        }
        """;

    /// <summary>
    /// HTO with nullable value-type properties.
    /// </summary>
    internal const string HtoWithNullableProperties = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Nullable", Classes = ["Nullable"])]
        public class HypermediaNullableHto : HypermediaObject
        {
            public int? OptionalCount { get; set; }
            public bool? OptionalFlag { get; set; }
            public DateTime? OptionalDate { get; set; }
        }
        """;

    /// <summary>
    /// HTO with enum properties including EnumMember attribute.
    /// </summary>
    internal const string HtoWithEnumProperties = $$"""
        {{Usings}}
        using System.Runtime.Serialization;
        {{AssemblyAttribute}}

        namespace TestHtos;

        public enum Status
        {
            Active,
            Inactive,
            Deleted
        }

        public enum Priority
        {
            [EnumMember(Value = "low")]
            Low,
            [EnumMember(Value = "medium")]
            Medium,
            [EnumMember(Value = "high")]
            High
        }

        [HypermediaObject(Title = "WithEnum", Classes = ["WithEnum"])]
        public class HypermediaWithEnumHto : HypermediaObject
        {
            public Status CurrentStatus { get; set; }
            public Priority CurrentPriority { get; set; }
        }
        """;

    /// <summary>
    /// HTO with collection and array properties.
    /// </summary>
    internal const string HtoWithCollections = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "WithCollections", Classes = ["WithCollections"])]
        public class HypermediaWithCollectionsHto : HypermediaObject
        {
            public string[] Tags { get; set; } = Array.Empty<string>();
            public List<int> Scores { get; set; } = new();
            public IEnumerable<bool> Flags { get; set; } = Array.Empty<bool>();
        }
        """;

    /// <summary>
    /// HTO with a nested complex object property.
    /// </summary>
    internal const string HtoWithNestedObject = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        public class Address
        {
            public string Street { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
        }

        [HypermediaObject(Title = "WithNested", Classes = ["WithNested"])]
        public class HypermediaWithNestedHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
            public Address HomeAddress { get; set; } = default!;
        }
        """;

    /// <summary>
    /// HTO demonstrating [HypermediaProperty(Name)] rename and [FormatterIgnoreHypermediaProperty] exclusion.
    /// </summary>
    internal const string HtoWithPropertyAttributes = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "WithAttributes", Classes = ["WithAttributes"])]
        public class HypermediaWithAttributesHto : HypermediaObject
        {
            [HypermediaProperty(Name = "FullName")]
            public string Name { get; set; } = string.Empty;

            [FormatterIgnoreHypermediaProperty]
            public string InternalId { get; set; } = string.Empty;

            public int Age { get; set; }
        }
        """;

    /// <summary>
    /// HTO with file upload action.
    /// </summary>
    internal const string HtoWithFileUpload = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        public class UploadAction : FileUploadHypermediaAction
        {
            public UploadAction() : base(() => true) { }
        }

        [HypermediaObject(Title = "Document", Classes = ["Document"])]
        public class HypermediaDocumentHto : HypermediaObject
        {
            public string Title { get; set; } = string.Empty;

            [HypermediaAction(Name = "Upload", Title = "Upload File")]
            public UploadAction? Upload { get; set; }
        }
        """;

    /// <summary>
    /// HTO with [Title] and [Description] attributes from JsonSchema.Net.Generation.
    /// </summary>
    internal const string HtoWithTitleDescriptionAttributes = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Address", Classes = ["Address"])]
        public class HypermediaAddressHto : HypermediaObject
        {
            public string Street { get; set; } = string.Empty;
        }

        public class MarkAsFavoriteAction : HypermediaAction
        {
            public MarkAsFavoriteAction() : base(() => true) { }
        }

        [Title("Customer Entity")]
        [Description("Represents a customer in the system.")]
        [HypermediaObject(Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Title("Best Friend Link")]
            [Description("Link to the customer's best friend.")]
            [Relations(["bestFriend"])]
            public ILink<HypermediaCustomerHto>? BestFriend { get; set; }

            [Title("Mark As Favorite")]
            [Description("Marks this customer as a favorite.")]
            [HypermediaAction]
            public MarkAsFavoriteAction? MarkAsFavorite { get; set; }

            [Title("Home Address")]
            [Description("The customer's home address.")]
            [Relations(["address"])]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }
        }
        """;

    /// <summary>
    /// HTO with XML doc comments for title/description harvesting.
    /// </summary>
    internal const string HtoWithXmlDocs = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Address", Classes = ["Address"])]
        public class HypermediaAddressHto : HypermediaObject
        {
            public string Street { get; set; } = string.Empty;
        }

        public class MarkAsFavoriteAction : HypermediaAction
        {
            public MarkAsFavoriteAction() : base(() => true) { }
        }

        /// <summary>A customer with profile and order history.</summary>
        /// <remarks>Represents an active customer account in the system.</remarks>
        [HypermediaObject(Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            /// <summary>Link to the customer's best friend.</summary>
            /// <remarks>Only present when a best friend is set.</remarks>
            [Relations(["bestFriend"])]
            public ILink<HypermediaCustomerHto>? BestFriend { get; set; }

            /// <summary>Marks this customer as a favorite.</summary>
            /// <remarks>Can only be executed by admins.</remarks>
            [HypermediaAction]
            public MarkAsFavoriteAction? MarkAsFavorite { get; set; }

            /// <summary>The customer's home address.</summary>
            /// <remarks>Primary residential address.</remarks>
            [Relations(["address"])]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }
        }
        """;

    /// <summary>
    /// HTO where [Title]/[Description] attributes override XML doc comments.
    /// </summary>
    internal const string HtoWithAttributeOverridingXmlDocs = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        /// <summary>XML doc title that should be overridden.</summary>
        /// <remarks>XML doc description that should be overridden.</remarks>
        [Title("Attribute Title")]
        [Description("Attribute Description")]
        [HypermediaObject(Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
        }
        """;

    /// <summary>
    /// HTO where [HypermediaObject(Title)] takes precedence over [Title] attribute.
    /// </summary>
    internal const string HtoWithHypermediaObjectTitleOverridingTitleAttribute = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [Title("Should Be Ignored")]
        [HypermediaObject(Title = "HypermediaObject Title", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
        }
        """;

    /// <summary>
    /// HTO with [Obsolete] on entity, link, action, and embedded entity.
    /// </summary>
    internal const string HtoWithDeprecation = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Address", Classes = ["Address"])]
        public class HypermediaAddressHto : HypermediaObject
        {
            public string Street { get; set; } = string.Empty;
        }

        public class MarkAsFavoriteAction : HypermediaAction
        {
            public MarkAsFavoriteAction() : base(() => true) { }
        }

        [Obsolete("Use HypermediaCustomerV2Hto instead.")]
        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Obsolete("Use preferredFriend instead.")]
            [Relations(["bestFriend"])]
            public ILink<HypermediaCustomerHto>? BestFriend { get; set; }

            [Obsolete]
            [HypermediaAction]
            public MarkAsFavoriteAction? MarkAsFavorite { get; set; }

            [Obsolete("Use primaryAddress instead.")]
            [Relations(["address"])]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }
        }
        """;

    /// <summary>
    /// HTO with 3rd-party attributes, RESTyard attributes, and XML doc comments
    /// for testing properties POCO generation.
    /// </summary>
    internal const string HtoWithMixedAttributes = $$"""
        {{Usings}}
        using System.Text.Json.Serialization;
        using RESTyard.AspNetCore.WebApi.RouteResolver;
        {{AssemblyAttribute}}

        namespace TestHtos;

        public class MyCustomConverter : JsonConverter<string>
        {
            public override string? Read(ref System.Text.Json.Utf8JsonReader reader, Type typeToConvert, System.Text.Json.JsonSerializerOptions options) => reader.GetString();
            public override void Write(System.Text.Json.Utf8JsonWriter writer, string value, System.Text.Json.JsonSerializerOptions options) => writer.WriteStringValue(value);
        }

        [HypermediaObject(Title = "Product", Classes = ["Product"])]
        public class HypermediaProductHto : HypermediaObject
        {
            /// <summary>The product display name.</summary>
            [JsonPropertyName("display_name")]
            [HypermediaProperty(Name = "DisplayName")]
            public string Name { get; set; } = string.Empty;

            [JsonConverter(typeof(MyCustomConverter))]
            public string SerialNumber { get; set; } = string.Empty;

            [FormatterIgnoreHypermediaProperty]
            public string InternalCode { get; set; } = string.Empty;

            [Key]
            public int Id { get; set; }

            /// <summary>The product price in USD.</summary>
            public decimal Price { get; set; }

            [Relations(["self"])]
            public ILink<HypermediaProductHto> Self { get; set; } = default!;

            [HypermediaAction]
            public MarkAction? Mark { get; set; }
        }

        public class MarkAction : HypermediaAction
        {
            public MarkAction() : base(() => true) { }
        }
        """;

    /// <summary>
    /// HTO combining properties, links, actions, and embedded entities.
    /// </summary>
    private const string UsingsWithAccessGroups = """
        using System;
        using System.Collections.Generic;
        using Json.Schema.Generation;
        using RESTyard.AspNetCore.Hypermedia;
        using RESTyard.AspNetCore.Hypermedia.Actions;
        using RESTyard.AspNetCore.Hypermedia.Attributes;
        using RESTyard.Schema.Model;
        """;

    /// <summary>
    /// HTO with access groups on entity, actions, links, and embedded entities.
    /// </summary>
    internal const string HtoWithAccessGroups = $$"""
        {{UsingsWithAccessGroups}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Address", Classes = ["Address"])]
        public class HypermediaAddressHto : HypermediaObject
        {
            public string Street { get; set; } = string.Empty;
        }

        public class DeleteAction : HypermediaAction
        {
            public DeleteAction() : base(() => true) { }
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        [HypermediaAccessGroup("admin", "sales")]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Relations(["self"])]
            public ILink<HypermediaCustomerHto> Self { get; set; } = default!;

            [Relations(["orders"])]
            [HypermediaAccessGroup("read")]
            public ILink<HypermediaCustomerHto>? Orders { get; set; }

            [HypermediaAction(Name = "DeleteCustomer")]
            [HypermediaAccessGroup("admin")]
            public DeleteAction? Delete { get; set; }

            [Relations(["address"])]
            [HypermediaAccessGroup("read", "write")]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }
        }
        """;

    /// <summary>
    /// HTO without any access groups — verifies null/absent behavior.
    /// </summary>
    internal const string HtoWithoutAccessGroups = $$"""
        {{UsingsWithAccessGroups}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Simple", Classes = ["Simple"])]
        public class HypermediaSimpleHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;

            [Relations(["self"])]
            public ILink<HypermediaSimpleHto> Self { get; set; } = default!;
        }
        """;

    internal const string FullHto = $$"""
        {{Usings}}
        {{AssemblyAttribute}}

        namespace TestHtos;

        [HypermediaObject(Title = "Address", Classes = ["Address"])]
        public class HypermediaAddressHto : HypermediaObject
        {
            public string Street { get; set; } = string.Empty;
            public string City { get; set; } = string.Empty;
        }

        public class MarkAsFavoriteAction : HypermediaAction
        {
            public MarkAsFavoriteAction() : base(() => true) { }
        }

        public class BuyCarParameters : IHypermediaActionParameter
        {
            public string CarId { get; set; } = string.Empty;
        }

        public class BuyCarAction : HypermediaAction<BuyCarParameters>
        {
            public BuyCarAction() : base() { }
        }

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }

            [Relations(["self"])]
            public ILink<HypermediaCustomerHto> Self { get; set; } = default!;

            [Relations(["bestFriend"])]
            public ILink<HypermediaCustomerHto>? BestFriend { get; set; }

            [HypermediaAction]
            public MarkAsFavoriteAction? MarkAsFavorite { get; set; }

            [HypermediaAction]
            public BuyCarAction? BuyCar { get; set; }

            [Relations(["address"])]
            public IEmbeddedEntity<HypermediaAddressHto>? Address { get; set; }
        }
        """;
}
