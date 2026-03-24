using System;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.Hypermedia.Links;
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

        return endpoints.MapGet(options.Route, (HttpContext context) =>
        {
            var schema = context.RequestServices.GetRequiredService<HypermediaApiSchema>();
            var json = JsonSerializer.Serialize(schema, SerializerOptions);

            context.Response.ContentType = SchemaMediaType;
            return context.Response.WriteAsync(json);
        }).WithName(RouteName);
    }

}
