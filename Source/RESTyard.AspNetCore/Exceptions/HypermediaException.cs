using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class HypermediaException : Exception
    {
        public HypermediaException(string description) : base(description)
        {
        }
    }
}