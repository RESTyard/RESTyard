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

    /// <summary>
    /// Minimal HTO with one string property.
    /// </summary>
    internal const string SimpleHto = $$"""
        {{Usings}}

        namespace TestHtos;

        [HypermediaObject(Title = "Customer", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
            public int Age { get; set; }
        }
        """;

    /// <summary>
    /// HTO with mandatory and optional links.
    /// </summary>
    internal const string HtoWithLinks = $$"""
        {{Usings}}

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

        namespace TestHtos;

        [Title("Should Be Ignored")]
        [HypermediaObject(Title = "HypermediaObject Title", Classes = ["Customer"])]
        public class HypermediaCustomerHto : HypermediaObject
        {
            public string Name { get; set; } = string.Empty;
        }
        """;

    /// <summary>
    /// HTO combining properties, links, actions, and embedded entities.
    /// </summary>
    internal const string FullHto = $$"""
        {{Usings}}

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
