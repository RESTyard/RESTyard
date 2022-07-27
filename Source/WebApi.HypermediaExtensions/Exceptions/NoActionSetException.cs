using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class NoActionSetException : Exception
    {
        public NoActionSetException(string message) : base(message)
        {
        }
    }
}