using System;

namespace WebApiHypermediaExtensionsCore.Exceptions
{
    public class ActionParameterValidationException : Exception
    {
        public ActionParameterValidationException(string message): base(message)
        {
        }
    }
}