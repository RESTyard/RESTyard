using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren action — a behavior available on an entity, described by its name, method, href, and fields.
/// <para>
/// Required fields per Siren spec: <see cref="Name"/> and <see cref="Href"/>.
/// </para>
/// </summary>
public class SirenAction
{
    /// <summary>
    /// A string that identifies the action to be performed. Must be unique within the set of actions for an entity.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// The URI of the action.
    /// </summary>
    [JsonPropertyName("href")]
    public required string Href { get; set; }

    /// <summary>
    /// Describes the nature of the action based on the current representation.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// Descriptive text about the action.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// An enumerated attribute mapping to a protocol method. Default is GET per the Siren spec.
    /// </summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>
    /// The encoding type for the request. Default is <c>application/x-www-form-urlencoded</c> per the Siren spec.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// The fields of the action, representing controls inside the action.
    /// </summary>
    [JsonPropertyName("fields")]
    public IList<SirenField>? Fields { get; set; }
}
