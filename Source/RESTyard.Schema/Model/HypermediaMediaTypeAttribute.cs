using System;
using System.Collections.Generic;

namespace RESTyard.Schema.Model;

/// <summary>
/// Declares the expected media type(s) of the resource behind a link property.
/// Used by the schema source generator to populate <c>MediaTypes</c> on
/// <see cref="LinkDescription"/>, and by the generated Siren mapper as the link
/// <c>type</c> when the reference does not set media types at runtime
/// (e.g. via <c>WithAvailableMediaTypes</c>).
/// <para>
/// Intended primarily for <c>ExternalLink</c> properties (downloads, external resources)
/// where the media type cannot be derived from a target entity type. Links without this
/// attribute get the Siren media type (<c>application/vnd.siren+json</c>) in the schema's
/// <c>mediaTypes</c> and no <c>type</c> in the rendered Siren output.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [Relations(["invoice-document"])]
/// [HypermediaMediaType("application/pdf", "text/html")]
/// public ExternalLink InvoiceDocument { get; init; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HypermediaMediaTypeAttribute : Attribute
{
    /// <summary>
    /// The media types the linked resource may be served as (e.g. <c>application/pdf</c>).
    /// </summary>
    public IReadOnlyList<string> MediaTypes { get; }

    /// <summary>
    /// Declares the expected media type(s) of the linked resource.
    /// </summary>
    /// <param name="mediaTypes">One or more media types (e.g. <c>application/pdf</c>).</param>
    public HypermediaMediaTypeAttribute(params string[] mediaTypes)
    {
        MediaTypes = mediaTypes;
    }
}
