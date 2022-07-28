using System;

namespace RESTyard.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaCommandAttribute : Attribute
    {
        public HypermediaCommandAttribute(string commandName)
        {
            this.Name = commandName;
        }

        public string Name { get; private set; }
    }
}