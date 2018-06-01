using System;

namespace HypermediaClient.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaCommandAttribute : Attribute
    {
        public HypermediaCommandAttribute(string commandName)
        {
            Name = commandName;
        }

        public string Name { get; private set; }
    }
}