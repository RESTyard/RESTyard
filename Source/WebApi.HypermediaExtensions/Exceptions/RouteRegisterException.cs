using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class RouteRegisterException : HypermediaException
    {
        public RouteRegisterException(string description) : base(description)
        {
        }
    }
}