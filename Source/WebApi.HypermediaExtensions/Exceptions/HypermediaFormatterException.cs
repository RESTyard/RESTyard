using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class HypermediaFormatterException : HypermediaException
    {
        public HypermediaFormatterException(string description) : base(description)
        {
        }
    }
}