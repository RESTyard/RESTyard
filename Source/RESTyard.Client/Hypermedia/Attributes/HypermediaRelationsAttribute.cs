using System;
using RESTyard.Client.Util;

namespace RESTyard.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaRelationsAttribute : Attribute
    {
        public HypermediaRelationsAttribute(params string[] relations)
        {
            this.Relations = new DistinctOrderedStringCollection(relations);
        }

        public IDistinctOrderedCollection<string> Relations { get; }
    }
}
