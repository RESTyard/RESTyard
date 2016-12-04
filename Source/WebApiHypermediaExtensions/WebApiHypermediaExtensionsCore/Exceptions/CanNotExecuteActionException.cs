using System;

namespace WebApiHypermediaExtensionsCore.Exceptions
{
    public class CanNotExecuteActionException : Exception
    {
        public CanNotExecuteActionException(string message) : base(message)
        {
        }
    }
}