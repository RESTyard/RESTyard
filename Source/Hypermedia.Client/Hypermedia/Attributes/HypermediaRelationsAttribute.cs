using System;
using Bluehands.Hypermedia.Client.Util;

namespace Bluehands.Hypermedia.Client.Hypermedia.Attributes
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
