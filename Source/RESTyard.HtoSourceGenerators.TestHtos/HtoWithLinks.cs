using System;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Attributes;
using RESTyard.AspNetCore.Hypermedia.Links;

namespace RESTyard.HtoSourceGenerators.TestHtos;

/// <summary>
/// Covers SirenConverter link code paths:
/// - Internal link (Link.To)
/// - Nullable link (skip when null)
/// - External link with media type (ExternalReference + AvailableMediaType)
/// - Self link
/// - Link deduplication (SirenConverter deduplicates by rel)
/// </summary>
[HypermediaObject(Title = "Links Test", Classes = ["LinksTest"])]
public class HtoWithAllLinkTypes : HypermediaObject
{
    public string Label { get; set; } = string.Empty;

    /// <summary>Internal link via Link.To.</summary>
    [Relations(["related"])]
    public ILink<SimpleCustomerHto> Related { get; set; }

    /// <summary>Nullable link — omitted when None.</summary>
    [Relations(["optional"])]
    public ILink<SimpleCustomerHto>? Optional { get; set; }

    /// <summary>External link with media type.</summary>
    [Relations(["external"])]
    public ILink<ExternalReference> ExternalSite { get; set; }

    [Relations(["self"])]
    public ILink<HtoWithAllLinkTypes> Self { get; set; }

    public HtoWithAllLinkTypes(
        SimpleCustomerHto relatedHto,
        ILink<SimpleCustomerHto>? optional,
        Uri externalUri,
        string externalMediaType)
    {
        Related = Link.To(relatedHto);
        Optional = optional;
        ExternalSite = Link.To(new ExternalReference(externalUri).WithAvailableMediaType(externalMediaType));
        Self = Link.To(this);
    }
}
