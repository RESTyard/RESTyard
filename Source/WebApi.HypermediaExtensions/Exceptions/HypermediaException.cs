using System;

namespace WebApi.HypermediaExtensions.Exceptions
{
    public class HypermediaException : Exception
    {
        public HypermediaException(string description) : base(description)
        {
        }
    }
}