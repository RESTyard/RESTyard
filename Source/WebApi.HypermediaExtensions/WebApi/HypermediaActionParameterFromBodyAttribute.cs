using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.WebApi.Extensions.JsonSchema;
using RESTyard.WebApi.Extensions.WebApi.ExtensionMethods;
using RESTyard.WebApi.Extensions.WebApi.RouteResolver;

namespace RESTyard.WebApi.Extensions.WebApi
{
    /// <summary>
    /// Explicitly marks an hpyermedia action parameter of controller method to be deserailized from body taking into account properties
    /// attributed with <see cref="KeyFromUriAttribute" />. This is not neccessary if <see cref="StartupExtensions.AddHypermediaParameterBinders"/>
    /// if called with forAttributedActionParametersOnly=false.
    /// </summary>
    public class HypermediaActionParameterFromBodyAttribute : ModelBinderAttribute
    {
        public HypermediaActionParameterFromBodyAttribute()
        {
            BinderType = typeof(HypermediaParameterFromBodyBinder);
        }
    }
}