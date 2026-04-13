using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren embedded representation sub-entity — a full inline entity nested within a parent entity.
/// Inherits all entity fields (<c>class</c>, <c>title</c>, <c>properties</c>, <c>entities</c>, <c>actions</c>, <c>links</c>)
/// from <see cref="SirenEntity{TProperties}"/> and adds the required <see cref="Rel"/> contract via
/// <see cref="ISirenSubEntity"/>.
/// <para>
/// Use <c>object</c> as <typeparamref name="TProperties"/> when the property type is not known at compile time.
/// </para>
/// </summary>
/// <typeparam name="TProperties">The type of the properties bag for this embedded entity.</typeparam>
public class SirenEmbeddedEntity<TProperties> : SirenEntity<TProperties>, ISirenSubEntity
{
    /// <summary>
    /// The relationship of the embedded entity to its parent, expressed as an array of link relation types.
    /// </summary>
    [JsonPropertyName("rel")]
    public required IReadOnlyList<string> Rel { get; set; }
}
