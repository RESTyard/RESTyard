using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class InvalidLinkException : Exception
    {
        public InvalidLinkException(string message) : base(message)
        {
        }
    }
}