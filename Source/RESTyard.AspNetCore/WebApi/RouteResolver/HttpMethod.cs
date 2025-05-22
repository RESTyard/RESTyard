using System;
using Microsoft.AspNetCore.Http;

namespace RESTyard.AspNetCore.WebApi.RouteResolver
{
    [Obsolete($"Please use Microsoft.AspNetCore.Http.{nameof(HttpMethods)}")]
    public enum HttpMethod
    {
        Undefined = 0,

        GET = 1,
        
        POST = 2,
        
        DELETE = 3,
        
        PATCH = 4,
        
        PUT = 5
    }
}
