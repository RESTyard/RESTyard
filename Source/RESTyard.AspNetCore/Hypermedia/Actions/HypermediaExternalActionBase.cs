using System;
using RESTyard.AspNetCore.WebApi.RouteResolver;

namespace RESTyard.AspNetCore.Hypermedia.Actions
{
    public abstract class HypermediaExternalActionBase : HypermediaActionBase
    {
        // from default attributes  Name and Title
        protected HypermediaExternalActionBase(Func<bool> canExecute, Uri externalUri, string httpMethod, string acceptedMediaType)
            : base(canExecute)
        {
            ExternalUri = externalUri;
            HttpMethod = httpMethod;
            AcceptedMediaType = acceptedMediaType;
        }
        
        protected HypermediaExternalActionBase(Func<bool> canExecute, Uri externalUri, string httpMethod) : base(canExecute)
        {
            ExternalUri = externalUri;
            HttpMethod = httpMethod;
        }

        public Uri ExternalUri { get; private set; }
        
        public string HttpMethod { get; private set; }

        public string? AcceptedMediaType { get; private set; }

    }
}