using Microsoft.AspNetCore.Mvc;
using WebApi.HypermediaExtensions.JsonSchema;
using WebApi.HypermediaExtensions.WebApi.ExtensionMethods;
using WebApi.HypermediaExtensions.WebApi.RouteResolver;

namespace WebApi.HypermediaExtensions.WebApi
{
    /// <summary>
    /// Explicitly marks an hpyermedia action parameter of controller method to be deserailized from body taking into account properties
    /// attributed with <see cref="KeyFromUriAttribute" />. This is not neccessary if <see cref="MvcOptionsExtensions.AddHypermediaParameterBinders"/>
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