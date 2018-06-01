using System;

namespace HypermediaClient.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaCommandParameterAttribute : Attribute
    {
        public HypermediaCommandParameterAttribute(string[] parameterClasses)
        {
            Classes = parameterClasses;
        }

        public string[] Classes { get; private set; }
    }
}