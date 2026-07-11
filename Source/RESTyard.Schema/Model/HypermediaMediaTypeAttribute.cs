using System;

namespace RESTyard.Schema.Model;

/// <summary>
/// Declares the expected media type of the resource behind a link property.
/// Used by the schema source generator to populate <c>MediaType</c> on
/// <see cref="LinkDescription"/>.
/// <para>
/// Intended primarily for <c>ExternalLink</c> properties (downloads, external resources)
/// where the media type cannot be derived from a target entity type. The value is purely
/// descriptive metadata for clients and tooling — it is not enforced at runtime.
/// </para>
/// </summary>
/// <example>
/// <code>
/// [Relations(["invoice-pdf"])]
/// [HypermediaMediaType("application/pdf")]
/// public ExternalLink InvoiceDocument { get; init; }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class HypermediaMediaTypeAttribute : Attribute
{
    /// <summary>
    /// The media type of the linked resource (e.g. <c>application/pdf</c>).
    /// </summary>
    public string MediaType { get; }

    /// <summary>
    /// Declares the expected media type of the linked resource.
    /// </summary>
    /// <param name="mediaType">The media type (e.g. <c>application/pdf</c>).</param>
    public HypermediaMediaTypeAttribute(string mediaType)
    {
        MediaType = mediaType;
    }
}
