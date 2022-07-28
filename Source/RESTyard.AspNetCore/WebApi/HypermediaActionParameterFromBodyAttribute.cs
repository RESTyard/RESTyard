using System;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.WebApi
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