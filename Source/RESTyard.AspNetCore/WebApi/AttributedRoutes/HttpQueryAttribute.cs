using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.Mvc.Routing;

namespace RESTyard.AspNetCore.WebApi.AttributedRoutes;

/// <summary>
/// Identifies an action that supports the HTTP QUERY method.
/// Placeholder until ASP.NET Core ships a built-in <c>[HttpQuery]</c> attribute.
/// </summary>
public class HttpQueryAttribute : HttpMethodAttribute
{
    private static readonly IEnumerable<string> SupportedMethods = [Microsoft.AspNetCore.Http.HttpMethods.Query];

    /// <summary>
    /// Creates a new <see cref="HttpQueryAttribute"/>.
    /// </summary>
    public HttpQueryAttribute() : base(SupportedMethods)
    {
    }

    /// <summary>
    /// Creates a new <see cref="HttpQueryAttribute"/> with the given route template.
    /// </summary>
    /// <param name="template">The route template. May not be null.</param>
    public HttpQueryAttribute([StringSyntax("Route")] string template)
        : base(SupportedMethods, template)
    {
        ArgumentNullException.ThrowIfNull(template);
    }
}
