using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren linked sub-entity — a reference to a related entity by href,
/// without embedding the full representation.
/// <para>
/// Required fields per Siren spec: <see cref="SirenSubEntity.Rel"/> and <see cref="Href"/>.
/// </para>
/// </summary>
public class SirenLinkedEntity : SirenSubEntity
{
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
