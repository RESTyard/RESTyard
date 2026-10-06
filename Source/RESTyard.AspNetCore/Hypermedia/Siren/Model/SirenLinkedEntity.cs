using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren linked sub-entity — a reference to a related entity by href,
/// without embedding the full representation.
/// <para>
/// Required fields per Siren spec: <see cref="Rel"/> and <see cref="Href"/>.
/// </para>
/// </summary>
public class SirenLinkedEntity : ISirenSubEntity
{
    /// <summary>
    /// The relationship of the linked entity to its parent, expressed as an array of link relation types.
    /// </summary>
    [JsonPropertyName("rel")]
    public required IReadOnlyList<string> Rel { get; set; }

    /// <summary>
    /// Describes the nature of the linked sub-entity based on the current representation.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// Descriptive text about the linked sub-entity.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// The URI of the linked sub-entity.
    /// </summary>
    [JsonPropertyName("href")]
    public required string Href { get; set; }

    /// <summary>
    /// The media type of the linked sub-entity, per Web Linking (RFC 5988).
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }
}
