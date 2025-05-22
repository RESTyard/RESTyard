using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.JsonSchema;
using RESTyard.AspNetCore.WebApi.ExtensionMethods;

namespace RESTyard.AspNetCore.WebApi
{
    /// <summary>
    /// Explicitly marks an hypermedia action parameter of controller method to be deserialized from body. This is not necessary if <see cref="StartupExtensions.AddHypermediaParameterBinders"/>
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