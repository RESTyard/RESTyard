using System;

namespace RESTyard.AspNetCore.Exceptions
{
    public class AttributedEnumTypeConverterException : Exception
    {
        public AttributedEnumTypeConverterException(string message, Exception exception = null) : base(message, exception)
        {
        }
    }
}