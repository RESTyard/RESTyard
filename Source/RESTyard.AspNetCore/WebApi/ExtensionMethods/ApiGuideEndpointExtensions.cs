using System;
using System.IO;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Extension methods for mapping the API guide (manual) endpoint.
/// </summary>
public static class ApiGuideEndpointExtensions
{
    /// <summary>
    /// The route name for the guide endpoint. Use with <see cref="Hypermedia.InternalReference"/>
    /// to create links to the guide from HTOs.
    /// </summary>
    public const string RouteName = "Restyard_ApiGuide";

    /// <summary>
    /// Maps a GET endpoint that serves an authored API guide (Markdown file) with media type
    /// <c>text/vnd.restyard.api-guide+markdown</c>. Default route: <c>/api-guide</c>.
    /// A relative <paramref name="filePath"/> is resolved against the application content root.
    /// The file must exist when the endpoint is mapped (fail-fast on misconfiguration).
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="filePath">Path to the Markdown guide file; relative paths resolve against the content root.</param>
    /// <param name="configure">Optional configuration for the endpoint (route, cache headers).</param>
    /// <returns>The route handler builder for further configuration (e.g., authorization).</returns>
    public static IEndpointConventionBuilder MapApiGuide(
        this IEndpointRouteBuilder endpoints,
        string filePath,
        Action<ApiGuideEndpointOptions>? configure = null)
    {
        var environment = endpoints.ServiceProvider.GetRequiredService<IHostEnvironment>();
        var resolvedPath = Path.IsPathRooted(filePath)
            ? filePath
            : Path.Combine(environment.ContentRootPath, filePath);

        if (!File.Exists(resolvedPath))
        {
            throw new FileNotFoundException(
                $"API guide file not found: '{resolvedPath}'. " +
                "Ensure the file exists and is copied to the output directory, " +
                "or use the IApiGuideProvider overload for dynamic content.",
                resolvedPath);
        }

        return endpoints.MapApiGuideCore(
            context => File.ReadAllTextAsync(resolvedPath, context.RequestAborted),
            configure);
    }

    /// <summary>
    /// Maps a GET endpoint that serves an authored API guide from an <see cref="IApiGuideProvider"/>
    /// with media type <c>text/vnd.restyard.api-guide+markdown</c>. Use for dynamic content
    /// (per-user, localized, assembled at request time). Default route: <c>/api-guide</c>.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="provider">The guide content provider; may be resolved from DI by the caller.</param>
    /// <param name="configure">Optional configuration for the endpoint (route, cache headers).</param>
    /// <returns>The route handler builder for further configuration (e.g., authorization).</returns>
    public static IEndpointConventionBuilder MapApiGuide(
        this IEndpointRouteBuilder endpoints,
        IApiGuideProvider provider,
        Action<ApiGuideEndpointOptions>? configure = null)
    {
        return endpoints.MapApiGuideCore(provider.GetGuideAsync, configure);
    }

    private static IEndpointConventionBuilder MapApiGuideCore(
        this IEndpointRouteBuilder endpoints,
        Func<HttpContext, System.Threading.Tasks.Task<string>> getContent,
        Action<ApiGuideEndpointOptions>? configure)
    {
        var options = new ApiGuideEndpointOptions();
        configure?.Invoke(options);

        return endpoints.MapGet(options.Route, async (HttpContext context) =>
        {
            var content = await getContent(context);
            CacheControlHeader.Apply(context, options.CacheMaxAge, options.CacheVisibility);
            context.Response.ContentType = DefaultMediaTypes.ApiGuide;
            await context.Response.WriteAsync(content);
        }).WithName(RouteName);
    }
}
