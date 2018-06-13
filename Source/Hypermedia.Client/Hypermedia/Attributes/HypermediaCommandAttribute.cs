namespace Hypermedia.Client.Hypermedia.Attributes
{
    using System;

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