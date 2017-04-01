using System;

namespace WebApiHypermediaExtensionsCore.Exceptions
{
    public class NoActionSetException : Exception
    {
        public NoActionSetException(string message) : base(message)
        {
        }
    }
}