using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.Net.Http.Headers;

namespace RESTyard.AspNetCore.WebApi.ExtensionMethods;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseFunctionResultInlining(this IApplicationBuilder app)
    {
        return app
            .Use(ReinvokePipelineOnFunctionResultLocationIfClientSupportsIt)
            .UseRewriter(CreateRewriteToResolveFunctionResult());
        
        async Task ReinvokePipelineOnFunctionResultLocationIfClientSupportsIt(HttpContext context, RequestDelegate next)
        {
            await next(context);
            if (context.Request.Headers.TryGetValue("X-RestyardInlineFunctionResult", out var resolve)
                && resolve == "true"
                && context.Response is { HasStarted: false, Headers.Location.Count: 1 })
            {
                var location = context.Response.Headers.Location[0]!;
                context.Response.Headers.Remove(HeaderNames.Location);
                context.Response.Headers["X-RestyardInlinedFunctionResult"] = location;
                context.Features.Set(new RewriteRequestToFunctionResultLocationFeature(
                    Method: "GET",
                    Location: location));
                await next(context);
            }
        }

        RewriteOptions CreateRewriteToResolveFunctionResult()
        {
            var rewriteOptions = new RewriteOptions();
            rewriteOptions.Add(context =>
            {
                var feature = context.HttpContext.Features.Get<RewriteRequestToFunctionResultLocationFeature>();
                if (feature is null)
                {
                    return;
                }

                context.HttpContext.Request.Method = feature.Method;
                UriHelper.FromAbsolute(feature.Location, out _, out _, out var path, out var query, out _);
                context.HttpContext.Request.Path = path;
                context.HttpContext.Request.QueryString = query;
            });
            return rewriteOptions;
        }
    }

    private record RewriteRequestToFunctionResultLocationFeature(string Method, string Location);
}