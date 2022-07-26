using System;
using RESTyard.Client.Util;

namespace RESTyard.Client.Hypermedia.Attributes
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
