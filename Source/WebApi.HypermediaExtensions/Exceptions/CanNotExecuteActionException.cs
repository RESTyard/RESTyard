using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class CanNotExecuteActionException : Exception
    {
        public CanNotExecuteActionException(string message) : base(message)
        {
        }
    }
}