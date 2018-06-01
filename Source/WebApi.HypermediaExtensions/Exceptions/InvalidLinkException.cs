using System;

namespace WebApi.HypermediaExtensions.Exceptions
{
    public class InvalidLinkException : Exception
    {
        public InvalidLinkException(string message) : base(message)
        {
        }
    }
}