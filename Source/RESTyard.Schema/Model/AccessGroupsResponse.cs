using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace RESTyard.Schema.Model;

/// <summary>
/// Response body for the access groups discovery endpoint.
/// </summary>
public class AccessGroupsResponse
{
    /// <summary>
    /// The access groups visible to the current user.
    /// </summary>
    [JsonPropertyName("accessGroups")]
    public IReadOnlyList<string> AccessGroups { get; set; } = Array.Empty<string>();
}
