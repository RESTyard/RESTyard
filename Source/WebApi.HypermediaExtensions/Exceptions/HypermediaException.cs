using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class HypermediaException : Exception
    {
        public HypermediaException(string description) : base(description)
        {
        }
    }
}