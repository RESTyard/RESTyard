using System;

namespace WebApiHypermediaExtensionsCore.Exceptions
{
    public class InvalidLinkException : Exception
    {
        public InvalidLinkException(string message) : base(message)
        {
        }
    }
}