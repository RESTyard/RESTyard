using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class RouteResolverException : HypermediaException
    {
        public RouteResolverException(string description) : base(description)
        {
        }
    }
}