using System;
using System.Collections.Generic;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// Covers SirenConverter embedded entity code paths (SirenAddEntities):
/// - Single resolved embedded entity → inline (calls CreateSirenInternal recursively)
/// - Collection of resolved embedded entities → multiple inline entities
/// - Nullable single embedded entity (skip when null)
/// - Unresolved internal → SirenLinkedEntity with class + href from resolver
/// - Unresolved external → SirenLinkedEntity with "External" class + URI
/// </summary>
[HypermediaObject(Title = "Embedded Test", Classes = ["EmbeddedTest"])]
public class HtoWithAllEmbeddedTypes : HypermediaObject
{
    public string Description { get; set; } = string.Empty;

    /// <summary>Single resolved embedded entity.</summary>
    [Relations(["primaryAddress"])]
    public IEmbeddedEntity<AddressHto>? PrimaryAddress { get; set; }

    /// <summary>Nullable single — should be skipped when null.</summary>
    [Relations(["secondaryAddress"])]
    public IEmbeddedEntity<AddressHto>? SecondaryAddress { get; set; }

    /// <summary>Collection of resolved embedded entities.</summary>
    [Relations(["customers"])]
    public List<IEmbeddedEntity<SimpleCustomerHto>> Customers { get; set; } = [];

    [Relations(["self"])]
    public ILink<HtoWithAllEmbeddedTypes> Self { get; set; }

    public HtoWithAllEmbeddedTypes()
    {
        Self = Link.To(this);
    }
}
