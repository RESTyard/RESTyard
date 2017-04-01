using System;

namespace HypermediaClient.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaClientObjectAttribute : Attribute
    {
        public string[] Classes { get; set; }
    }
}
