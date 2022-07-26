using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class HypermediaRouteException : HypermediaException
    {
        public HypermediaRouteException(string description) : base(description)
        {
        }
    }
}