using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class ActionParameterValidationException : Exception
    {
        public ActionParameterValidationException(string message): base(message)
        {
        }
    }
}