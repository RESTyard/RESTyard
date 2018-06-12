namespace Hypermedia.Client.Hypermedia.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaRelationsAttribute : Attribute
    {
        public HypermediaRelationsAttribute(string[] relations)
        {
            this.Relations = relations;
        }

        public string[] Relations { get; private set; }
    }
}
