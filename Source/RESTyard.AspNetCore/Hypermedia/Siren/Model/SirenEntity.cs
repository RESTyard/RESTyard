using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren entity — a URI-addressable resource with properties, actions, links, and sub-entities.
/// Returned by generated <c>ToSiren()</c> methods.
/// <para>
/// <typeparamref name="TProperties"/> is typically a generated properties POCO per HTO
/// that carries all forwarded attributes (serializer attributes, <c>[JsonConverter]</c>, etc.).
/// Use <c>object</c> when the property type is not known at compile time.
/// </para>
/// <para>
/// Conforms to the <see href="https://github.com/kevinswiber/siren">Siren specification</see>.
/// Intentional deviation: the spec defines <c>properties</c> as an untyped object.
/// The generic type parameter adds compile-time type safety without changing the JSON wire format.
/// </para>
/// </summary>
/// <typeparam name="TProperties">The type of the properties bag for this entity.</typeparam>
public class SirenEntity<TProperties>
{
    /// <summary>
    /// Describes the nature of the entity based on the current representation.
    /// Possible values are implementation-dependent and should be documented.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// Descriptive text about the entity.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// A set of key-value pairs that describe the state of the entity.
    /// </summary>
    [JsonPropertyName("properties")]
    public TProperties? Properties { get; set; }

    /// <summary>
    /// Navigation links that connect this entity to related resources.
    /// </summary>
    [JsonPropertyName("links")]
    public IList<SirenLink>? Links { get; set; }

    /// <summary>
    /// Available actions that can be performed on this entity.
    /// </summary>
    [JsonPropertyName("actions")]
    public IList<SirenAction>? Actions { get; set; }

    /// <summary>
    /// Related sub-entities — either embedded representations or linked sub-entities.
    /// </summary>
    [JsonPropertyName("entities")]
    public IList<SirenSubEntity>? Entities { get; set; }
}
