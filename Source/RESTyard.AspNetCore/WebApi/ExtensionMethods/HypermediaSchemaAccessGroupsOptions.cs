using System;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Options for the access groups discovery endpoint.
/// </summary>
public class HypermediaSchemaAccessGroupsOptions
{
    /// <summary>
    /// Route for the access groups endpoint. Default: <c>/schema/access-groups</c>.
    /// </summary>
    public string Route { get; set; } = "/schema/access-groups";

    /// <summary>
    /// When set, a <c>Cache-Control: private, max-age=…</c> header is emitted.
    /// Default: <c>null</c> — no cache header (opt-in).
    /// This endpoint is always <c>private</c>: the response varies per caller when a sanitizer is
    /// registered, and a shared cache serving one caller's groups to another would leak data —
    /// so there is deliberately no <see cref="CacheVisibility"/> option here.
    /// </summary>
    public TimeSpan? CacheMaxAge { get; set; }
}
