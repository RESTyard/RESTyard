using System.Collections.Generic;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Hook that sanitizes access group filter requests based on the current user's context.
/// Register via DI to control which access groups a user is allowed to query on the
/// <c>/hypermedia-schema</c> endpoint.
/// <para>
/// When no implementation is registered, requested access groups are passed through unchanged
/// (the schema endpoint is open).
/// </para>
/// </summary>
/// <example>
/// <code>
/// public class RoleBasedSanitizer : ISchemaAccessGroupSanitizer
/// {
///     public IReadOnlySet&lt;string&gt; SanitizeRequestedGroups(
///         IReadOnlySet&lt;string&gt; requestedGroups, HttpContext httpContext)
///     {
///         if (!httpContext.User.IsInRole("admin"))
///             return requestedGroups.Except(new[] { "admin" }).ToHashSet();
///         return requestedGroups;
///     }
/// }
/// </code>
/// </example>
public interface ISchemaAccessGroupSanitizer
{
    /// <summary>
    /// Given the access groups the client requested, returns the access groups
    /// the client is actually allowed to see. The server can remove groups
    /// the user is not permitted to query, or add implicit groups.
    /// </summary>
    /// <param name="requestedGroups">The access groups from the query parameter.</param>
    /// <param name="httpContext">The current HTTP context for inspecting user claims, roles, etc.</param>
    /// <returns>The sanitized set of access groups to use for filtering.</returns>
    IReadOnlySet<string> SanitizeRequestedGroups(
        IReadOnlySet<string> requestedGroups,
        HttpContext httpContext);
}
