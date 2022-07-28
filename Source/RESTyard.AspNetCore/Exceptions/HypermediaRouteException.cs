using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class HypermediaRouteException : HypermediaException
    {
        public HypermediaRouteException(string description) : base(description)
        {
        }
    }
}