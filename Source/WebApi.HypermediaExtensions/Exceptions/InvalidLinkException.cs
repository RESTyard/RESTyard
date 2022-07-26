using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class InvalidLinkException : Exception
    {
        public InvalidLinkException(string message) : base(message)
        {
        }
    }
}