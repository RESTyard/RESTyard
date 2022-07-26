﻿using System;

namespace RESTyard.WebApi.Extensions.Exceptions
{
    public class AttributedEnumTypeConverterException : Exception
    {
        public AttributedEnumTypeConverterException(string message, Exception exception = null) : base(message, exception)
        {
        }
    }
}