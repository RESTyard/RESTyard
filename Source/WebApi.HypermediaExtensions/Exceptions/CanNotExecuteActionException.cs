using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class CanNotExecuteActionException : Exception
    {
        public CanNotExecuteActionException(string message) : base(message)
        {
        }
    }
}