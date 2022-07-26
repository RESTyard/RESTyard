using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class HypermediaFormatterException : HypermediaException
    {
        public HypermediaFormatterException(string description) : base(description)
        {
        }
    }
}