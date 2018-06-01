using System;

namespace WebApi.HypermediaExtensions.Exceptions
{
    public class CanNotExecuteActionException : Exception
    {
        public CanNotExecuteActionException(string message) : base(message)
        {
        }
    }
}