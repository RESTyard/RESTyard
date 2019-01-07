using System;

namespace Bluehands.Hypermedia.Client.Hypermedia.Attributes
{
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
