using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren link — a navigational transition from the current entity to a related resource.
/// <para>
/// Required fields per Siren spec: <see cref="Rel"/> and <see cref="Href"/>.
/// </para>
/// </summary>
public class SirenLink
{
    /// <summary>
    /// The relationship of the link to the entity, expressed as an array of link relation types.
    /// </summary>
    [JsonPropertyName("rel")]
    public required IReadOnlyList<string> Rel { get; set; }

    /// <summary>
    /// The URI of the linked resource.
    /// </summary>
    [JsonPropertyName("href")]
    public required string Href { get; set; }

    /// <summary>
    /// Descriptive text about the link.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// Describes the nature of the link based on the current representation.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// The media type of the linked resource, per Web Linking (RFC 5988).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
