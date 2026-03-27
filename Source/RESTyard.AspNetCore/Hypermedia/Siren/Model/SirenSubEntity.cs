using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Base class for Siren sub-entities. A sub-entity is either an
/// <see cref="SirenEmbeddedEntity{TProperties}">embedded representation</see> (full inline entity) or a
/// <see cref="SirenLinkedEntity">linked sub-entity</see> (reference by href).
/// <para>
/// Required fields per Siren spec: <see cref="Rel"/>.
/// </para>
/// <para>
/// Serialization: <see cref="SirenSubEntityConverter"/> serializes derived type properties based on
/// runtime type. No type discriminator is added to the JSON because Siren distinguishes sub-entity types
/// structurally (embedded has inline entity properties, linked has <c>href</c>).
/// Deserialization from JSON requires a custom converter for structural discrimination.
/// </para>
/// </summary>
[JsonConverter(typeof(SirenSubEntityConverter))]
public abstract class SirenSubEntity
{
    /// <summary>
    /// The relationship of the sub-entity to its parent entity, expressed as an array of link relation types.
    /// </summary>
    [JsonPropertyName("rel")]
    public required IReadOnlyList<string> Rel { get; set; }

    /// <summary>
    /// Describes the nature of the sub-entity based on the current representation.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// Descriptive text about the sub-entity.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }
}
