using System;

namespace RESTyard.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaCommandParameterAttribute : Attribute
    {
        public HypermediaCommandParameterAttribute(string[] parameterClasses)
        {
            this.Classes = parameterClasses;
        }

        public string[] Classes { get; private set; }
    }
}