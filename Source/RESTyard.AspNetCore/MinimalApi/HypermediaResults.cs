using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using RESTyard.AspNetCore.Hypermedia;
using RESTyard.AspNetCore.WebApi.Formatter;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.MinimalApi;

public class HypermediaResults
{
    public static IResult Ok<THypermediaObject>(THypermediaObject hto)
        where THypermediaObject : HypermediaObject
    {
        return new Ok<THypermediaObject>(hto);
    }
}

public sealed class Ok<THypermediaObject> : IResult
    where THypermediaObject : HypermediaObject
{
    internal Ok(THypermediaObject hto)
    {
        Value = hto;
    }

    public THypermediaObject Value { get; }

    public Task ExecuteAsync(HttpContext httpContext)
    {
        var routeResolver = httpContext.RequestServices.GetRequiredService<IRouteResolverFactory>().CreateRouteResolver(httpContext);
        var converter = httpContext.RequestServices.GetRequiredService<ISirenHypermediaConverterFactory>()
            .CreateSirenConverter(routeResolver);
        var sirenJson = converter.ConvertToString(this.Value);

        httpContext.Response.ContentType = DefaultMediaTypes.Siren;
        httpContext.Response.ContentLength = Encoding.UTF8.GetByteCount(sirenJson);
        return httpContext.Response.WriteAsync(sirenJson);
    }
}