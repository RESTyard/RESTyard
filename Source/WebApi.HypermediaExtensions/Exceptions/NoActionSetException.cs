using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class NoActionSetException : Exception
    {
        public NoActionSetException(string message) : base(message)
        {
        }
    }
}