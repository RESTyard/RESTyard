using System;

namespace WebApi.HypermediaExtensions.Exceptions
{
    public class ActionParameterValidationException : Exception
    {
        public ActionParameterValidationException(string message): base(message)
        {
        }
    }
}