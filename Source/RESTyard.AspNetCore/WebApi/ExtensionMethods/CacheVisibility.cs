namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Cache visibility for the <c>Cache-Control</c> header emitted by the schema and guide endpoints
/// when <c>CacheMaxAge</c> is set.
/// </summary>
public enum CacheVisibility
{
    /// <summary>
    /// <c>Cache-Control: private</c> — only the caller's own cache may store the response.
    /// Safe in every mode; the default.
    /// </summary>
    Private,

    /// <summary>
    /// <c>Cache-Control: public</c> — shared caches (proxies, CDNs) may store the response.
    /// Setting this is an assertion that the response is identical for every caller.
    /// </summary>
    Public,
}
