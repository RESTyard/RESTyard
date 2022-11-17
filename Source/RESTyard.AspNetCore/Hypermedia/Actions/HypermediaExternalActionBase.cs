using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;
using RESTyard.MediaTypes;

namespace RESTyard.AspNetCore.Hypermedia.Actions
{
    public abstract class HypermediaExternalActionBase : HypermediaActionBase
    {
        // from default attributes  Name and Title
        protected HypermediaExternalActionBase(Func<bool> canExecute, Uri externalUri, HttpMethod httpMethod, string encodingType = DefaultMediaTypes.ApplicationJson) : base(canExecute)
        {
            ExternalUri = externalUri;
            HttpMethod = httpMethod;
            EncodingType = encodingType;
        }

        public Uri ExternalUri { get; private set; }
        
        public HttpMethod HttpMethod { get; private set; }
        
        public string EncodingType { get; private set; }
        
    }
}