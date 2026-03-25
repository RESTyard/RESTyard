using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.Schema;
using RESTyard.Schema.Model;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Extension methods for mapping the hypermedia schema endpoint.
/// </summary>
public static class HypermediaSchemaEndpointExtensions
{
    /// <summary>
    /// The route name for the schema endpoint. Use with <see cref="InternalReference"/>
    /// to create links to the schema from HTOs.
    /// </summary>
    public const string RouteName = "HypermediaSchema";

    /// <summary>
    /// The route name for the access groups discovery endpoint.
    /// </summary>
    public const string AccessGroupsRouteName = "HypermediaSchemaAccessGroups";

    private const string SchemaMediaType = SchemaMediaTypes.HypermediaApiSchema;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Maps a GET endpoint that serves the <see cref="HypermediaApiSchema"/> as JSON.
    /// Default route: <c>/hypermedia-schema</c>.
    /// Supports optional access group filtering via query parameters:
    /// <c>?accessGroups=read,write</c> (include mode) or <c>?excludeAccessGroups=admin</c> (exclude mode).
    /// </summary>
    /// <remarks>
    /// Requires <c>AddHypermediaSchema()</c> during service registration.
    /// </remarks>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">Optional configuration for the endpoint (route, etc.).</param>
    /// <returns>The route handler builder for further configuration (e.g., authorization).</returns>
    public static IEndpointConventionBuilder MapHypermediaSchema(
        this IEndpointRouteBuilder endpoints,
        Action<HypermediaSchemaEndpointOptions>? configure = null)
    {
        var options = new HypermediaSchemaEndpointOptions();
        configure?.Invoke(options);

        return endpoints.MapGet(options.Route, (
            HypermediaApiSchema schema,
            HttpContext context,
            string? accessGroups,
            string? excludeAccessGroups) =>
        {
            if (!string.IsNullOrEmpty(accessGroups) && !string.IsNullOrEmpty(excludeAccessGroups))
            {
                context.Response.StatusCode = StatusCodes.Status400BadRequest;
                context.Response.ContentType = "application/problem+json";
                var problem = JsonSerializer.Serialize(new
                {
                    type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    title = "Invalid query parameters",
                    status = 400,
                    detail = "Cannot specify both 'accessGroups' and 'excludeAccessGroups'. Use one or the other.",
                });
                return context.Response.WriteAsync(problem);
            }

            if (!string.IsNullOrEmpty(accessGroups))
            {
                var groups = SanitizeGroups(ParseAccessGroups(accessGroups), context);
                schema = HypermediaSchemaFilter.ForAccessGroups(schema, groups);
            }
            else if (!string.IsNullOrEmpty(excludeAccessGroups))
            {
                var groups = SanitizeGroups(ParseAccessGroups(excludeAccessGroups), context);
                schema = HypermediaSchemaFilter.ExcludeAccessGroups(schema, groups);
            }

            var json = JsonSerializer.Serialize(schema, SerializerOptions);
            context.Response.ContentType = SchemaMediaType;
            return context.Response.WriteAsync(json);
        }).WithName(RouteName);
    }

    private static HashSet<string> ParseAccessGroups(string commaSeparated)
    {
        return commaSeparated
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Applies the <see cref="ISchemaAccessGroupSanitizer"/> if registered in DI.
    /// When no sanitizer is registered, the groups are passed through unchanged.
    /// Returns a <see cref="HashSet{T}"/> to satisfy <see cref="HypermediaSchemaFilter"/>
    /// which requires <see cref="ISet{T}"/> (netstandard2.0 compatibility).
    /// </summary>
    private static HashSet<string> SanitizeGroups(HashSet<string> groups, HttpContext context)
    {
        var sanitizer = context.RequestServices.GetService<ISchemaAccessGroupSanitizer>();
        if (sanitizer == null)
        {
            return groups;
        }

        var sanitized = sanitizer.SanitizeRequestedGroups(groups, context);
        return sanitized as HashSet<string> ?? sanitized.ToHashSet(StringComparer.Ordinal);
    }

    /// <summary>
    /// Maps a GET endpoint that returns the access groups available to the current user.
    /// Default route: <c>/schema/access-groups</c>.
    /// Returns the <c>DeclaredAccessGroups</c> from the schema, filtered through
    /// <see cref="ISchemaAccessGroupSanitizer"/> if registered.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="configure">Optional configuration for the endpoint (route, etc.).</param>
    /// <returns>The route handler builder for further configuration (e.g., authorization).</returns>
    public static IEndpointConventionBuilder MapHypermediaSchemaAccessGroups(
        this IEndpointRouteBuilder endpoints,
        Action<HypermediaSchemaAccessGroupsOptions>? configure = null)
    {
        var options = new HypermediaSchemaAccessGroupsOptions();
        configure?.Invoke(options);

        return endpoints.MapGet(options.Route, (
            HypermediaApiSchema schema,
            HttpContext context) =>
        {
            var declaredGroups = schema.DeclaredAccessGroups ?? [];

            var sanitizer = context.RequestServices.GetService<ISchemaAccessGroupSanitizer>();
            IReadOnlyList<string> visibleGroups;
            if (sanitizer != null)
            {
                var asSet = declaredGroups.ToHashSet(StringComparer.Ordinal);
                var sanitized = sanitizer.SanitizeRequestedGroups(asSet, context);
                visibleGroups = sanitized.Order().ToList();
            }
            else
            {
                visibleGroups = declaredGroups;
            }

            var response = new AccessGroupsResponse { AccessGroups = visibleGroups };
            var json = JsonSerializer.Serialize(response, SerializerOptions);
            context.Response.ContentType = SchemaMediaTypes.HypermediaSchemaAccessGroups;
            return context.Response.WriteAsync(json);
        }).WithName(AccessGroupsRouteName);
    }
}
