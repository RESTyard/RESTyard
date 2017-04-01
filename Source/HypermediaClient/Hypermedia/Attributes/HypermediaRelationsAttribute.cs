using System;

namespace HypermediaClient.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaRelationsAttribute : Attribute
    {
        public HypermediaRelationsAttribute(string[] relations)
        {
            Relations = relations;
        }

        public string[] Relations { get; private set; }
    }
}
