using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class RouteResolverException : HypermediaException
    {
        public RouteResolverException(string description) : base(description)
        {
        }
    }
}