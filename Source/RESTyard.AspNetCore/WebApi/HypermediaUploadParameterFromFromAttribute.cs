using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTyard.AspNetCore.JsonSchema;

namespace RESTyard.AspNetCore.WebApi;

/// <summary>
/// Explicitly marks a hypermedia upload action parameter of controller method to be deserialized from the multipart/form-data form of the request
/// The parameter needs to be of type <see cref="HypermediaFileUploadActionParameter" /> or <see cref="HypermediaFileUploadActionParameter{TParameters}" />
/// </summary>
public class HypermediaUploadParameterFromFromAttribute : ModelBinderAttribute
{
    public HypermediaUploadParameterFromFromAttribute()
    {
        BinderType = typeof(HypermediaParameterFromFormBinder);
    }
}