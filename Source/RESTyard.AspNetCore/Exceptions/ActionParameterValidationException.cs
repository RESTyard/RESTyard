using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class ActionParameterValidationException : Exception
    {
        public ActionParameterValidationException(string message): base(message)
        {
        }
    }
}