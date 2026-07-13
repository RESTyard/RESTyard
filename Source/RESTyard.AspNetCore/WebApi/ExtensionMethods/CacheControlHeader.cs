using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

/// <summary>
/// Emits the opt-in <c>Cache-Control</c> header for the schema and guide endpoints.
/// </summary>
internal static class CacheControlHeader
{
    public static void Apply(HttpContext context, TimeSpan? maxAge, CacheVisibility visibility)
    {
        if (maxAge is not { } age)
        {
            return;
        }

        var visibilityValue = visibility == CacheVisibility.Public ? "public" : "private";
        context.Response.Headers.CacheControl = $"{visibilityValue}, max-age={(long)age.TotalSeconds}";
    }
}
