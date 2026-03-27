using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.AspNetCore.Hypermedia.Siren.Model;

/// <summary>
/// Represents a Siren field — a control inside an action, analogous to an HTML input element.
/// <para>
/// Required fields per Siren spec: <see cref="Name"/>.
/// </para>
/// </summary>
public class SirenField
{
    /// <summary>
    /// A name describing the control. Must be unique within the set of fields for an action.
    /// </summary>
    [JsonPropertyName("name")]
    public required string Name { get; set; }

    /// <summary>
    /// Describes the nature of the field based on the current representation.
    /// </summary>
    [JsonPropertyName("class")]
    public IReadOnlyList<string>? Class { get; set; }

    /// <summary>
    /// The input type of the field. Defaults to <c>text</c> per the Siren spec.
    /// RESTyard uses <c>application/json</c> for typed action parameters.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    /// <summary>
    /// Descriptive text about the field.
    /// </summary>
    [JsonPropertyName("title")]
    public string? Title { get; set; }

    /// <summary>
    /// A value assigned to the field. May contain prefilled/default values.
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    // --- RESTyard-specific extensions (not part of the Siren spec) ---

    /// <summary>
    /// RESTyard extension for file upload fields. Comma-separated list of accepted file specifiers
    /// (e.g., <c>.pdf,.jpg</c> or <c>image/*</c>).
    /// </summary>
    [JsonPropertyName("accept")]
    public string? Accept { get; set; }

    /// <summary>
    /// RESTyard extension for file upload fields. Maximum allowed file size in bytes.
    /// A value of <c>-1</c> or <c>null</c> means no limit.
    /// </summary>
    [JsonPropertyName("maxFileSizeBytes")]
    public long? MaxFileSizeBytes { get; set; }

    /// <summary>
    /// RESTyard extension for file upload fields. Whether multiple files can be uploaded in a single request.
    /// </summary>
    [JsonPropertyName("allowMultiple")]
    public bool? AllowMultiple { get; set; }
}
