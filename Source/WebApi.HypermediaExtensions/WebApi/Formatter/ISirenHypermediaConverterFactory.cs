using System;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi.Formatter
{
    public interface ISirenHypermediaConverterFactory
    {
        IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver);
    }
}