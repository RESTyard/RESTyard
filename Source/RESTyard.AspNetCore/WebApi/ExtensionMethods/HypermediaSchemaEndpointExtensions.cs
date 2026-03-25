using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
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

    private const string SchemaMediaType = SchemaMediaTypes.HypermediaApiSchema;

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };

    /// <summary>
    /// Maps a GET endpoint that serves the <see cref="HypermediaApiSchema"/> as JSON.
    /// Supports optional access group filtering via query parameters:
    /// <c>?accessGroups=read,write</c> (include mode) or <c>?excludeAccessGroups=admin</c> (exclude mode).
    /// </summary>
    /// <remarks>
    /// Make sure <c>AddHypermediaSchema()</c> was called during service registration
    /// to enable the schema feature.
    /// </remarks>
    /// <example>
    /// <code>
    /// app.MapHypermediaSchema();
    /// app.MapHypermediaSchema(o => o.Route = "/api/schema");
    /// </code>
    /// </example>
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
                schema = HypermediaSchemaFilter.ForAccessGroups(schema, ParseAccessGroups(accessGroups));
            }
            else if (!string.IsNullOrEmpty(excludeAccessGroups))
            {
                schema = HypermediaSchemaFilter.ExcludeAccessGroups(schema, ParseAccessGroups(excludeAccessGroups));
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
}
