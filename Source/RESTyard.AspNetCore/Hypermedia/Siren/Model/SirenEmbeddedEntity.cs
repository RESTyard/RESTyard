using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren embedded representation sub-entity — a full inline entity nested within a parent entity.
/// The embedded entity carries the full entity representation (properties, links, actions, entities)
/// plus the inherited <see cref="SirenSubEntity.Rel">relationship</see>,
/// <see cref="SirenSubEntity.Class">class</see>, and <see cref="SirenSubEntity.Title">title</see>.
/// <para>
/// Per the Siren spec, an embedded representation sub-entity is an Entity with an additional required <c>rel</c> field.
/// The entity fields are inlined (rather than using inheritance from <see cref="SirenEntity{TProperties}"/>)
/// because C# does not support multiple inheritance (<see cref="SirenSubEntity"/> is the base class for <c>rel</c>).
/// </para>
/// <para>
/// Use <c>object</c> as <typeparamref name="TProperties"/> when the property type is not known at compile time.
/// </para>
/// </summary>
/// <typeparam name="TProperties">The type of the properties bag for this embedded entity.</typeparam>
public class SirenEmbeddedEntity<TProperties> : SirenSubEntity
{
    /// <summary>
    /// A set of key-value pairs that describe the state of the embedded entity.
    /// </summary>
    [JsonPropertyName("properties")]
    public TProperties? Properties { get; set; }

    /// <summary>
    /// Navigation links for the embedded entity.
    /// </summary>
    [JsonPropertyName("links")]
    public IList<SirenLink>? Links { get; set; }

    /// <summary>
    /// Available actions on the embedded entity.
    /// </summary>
    [JsonPropertyName("actions")]
    public IList<SirenAction>? Actions { get; set; }

    /// <summary>
    /// Nested sub-entities of the embedded entity.
    /// </summary>
    [JsonPropertyName("entities")]
    public IList<SirenSubEntity>? Entities { get; set; }
}
