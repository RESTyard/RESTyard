using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class RouteRegisterException : HypermediaException
    {
        public RouteRegisterException(string description) : base(description)
        {
        }
    }
}