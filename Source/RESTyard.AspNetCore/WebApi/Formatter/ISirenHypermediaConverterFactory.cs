using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi.Formatter
{
    public interface ISirenHypermediaConverterFactory
    {
        IHypermediaConverter CreateSirenConverter(IHypermediaRouteResolver hypermediaRouteResolver);
    }
}