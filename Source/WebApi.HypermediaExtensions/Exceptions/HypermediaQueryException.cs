using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class HypermediaQueryException : HypermediaException
    {
        public HypermediaQueryException(string description) : base(description)
        {
        }
    }
}