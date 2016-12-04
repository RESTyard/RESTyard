using System;

namespace WebApiHypermediaExtensionsCore.Exceptions
{
    public class HypermediaException : Exception
    {
        public HypermediaException(string description) : base(description)
        {
        }
    }
}