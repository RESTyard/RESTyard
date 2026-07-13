using System;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Options for the API guide endpoint.
/// </summary>
public class ApiGuideEndpointOptions
{
    /// <summary>
    /// The route path for the guide endpoint.
    /// Default: <c>/api-guide</c>.
    /// </summary>
    public string Route { get; set; } = "/api-guide";

    /// <summary>
    /// When set, a <c>Cache-Control</c> header with this <c>max-age</c> is emitted.
    /// Default: <c>null</c> — no cache header (opt-in).
    /// </summary>
    public TimeSpan? CacheMaxAge { get; set; }

    /// <summary>
    /// Cache visibility used when <see cref="CacheMaxAge"/> is set. Default: <see cref="CacheVisibility.Private"/>.
    /// Set <see cref="CacheVisibility.Public"/> only if the guide content is identical for every caller
    /// (always true for the file-path overload; your call for an <see cref="IApiGuideProvider"/>).
    /// </summary>
    public CacheVisibility CacheVisibility { get; set; } = CacheVisibility.Private;
}
