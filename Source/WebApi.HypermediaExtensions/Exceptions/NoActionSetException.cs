using System;

namespace WebApi.HypermediaExtensions.Exceptions
{
    public class NoActionSetException : Exception
    {
        public NoActionSetException(string message) : base(message)
        {
        }
    }
}