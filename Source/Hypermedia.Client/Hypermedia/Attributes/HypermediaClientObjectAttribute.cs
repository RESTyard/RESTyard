using System;
using Bluehands.Hypermedia.Client.Util;

namespace Bluehands.Hypermedia.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaClientObjectAttribute : Attribute
    {
        public HypermediaClientObjectAttribute(params string[] classes)
        {
            this.Classes = new DistinctOrderedStringCollection(classes);
        }

        public IDistinctOrderedCollection<string> Classes { get; }
    }
}
