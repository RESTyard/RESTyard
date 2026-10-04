using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Contract for Siren sub-entities — items that may appear in a parent entity's
/// <see cref="SirenEntity{TProperties}.Entities"/> collection.
/// Implemented by <see cref="SirenEmbeddedEntity{TProperties}"/> (full inline entity)
/// and <see cref="SirenLinkedEntity"/> (href-only reference).
/// <para>
/// Required by Siren spec for sub-entities: <see cref="Rel"/>.
/// <see cref="Class"/> and <see cref="Title"/> are optional sub-entity descriptors.
/// </para>
/// <para>
/// Serialization: <see cref="SirenSubEntityConverter"/> handles polymorphic
/// (de)serialization since Siren uses structural — not type-discriminator — discrimination
/// (embedded has inline entity properties, linked has <c>href</c>).
/// </para>
/// </summary>
[JsonConverter(typeof(SirenSubEntityConverter))]
public interface ISirenSubEntity
{
    /// <summary>
    /// The relationship of the sub-entity to its parent entity, expressed as an array of link relation types.
    /// </summary>
    IReadOnlyList<string> Rel { get; set; }

    /// <summary>
    /// Describes the nature of the sub-entity based on the current representation.
    /// </summary>
    IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// Descriptive text about the sub-entity.
    /// </summary>
    string? Title { get; set; }
}
