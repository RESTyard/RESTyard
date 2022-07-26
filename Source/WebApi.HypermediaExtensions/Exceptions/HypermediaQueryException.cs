using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class HypermediaQueryException : HypermediaException
    {
        public HypermediaQueryException(string description) : base(description)
        {
        }
    }
}