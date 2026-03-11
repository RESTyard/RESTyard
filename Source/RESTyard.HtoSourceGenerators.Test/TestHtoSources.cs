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
            public HypermediaAddressHto? Address { get; set; }

            [Relations(["orders"])]
            public List<HypermediaAddressHto> Addresses { get; set; } = new();
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
            public HypermediaAddressHto? Address { get; set; }
        }
        """;
}
