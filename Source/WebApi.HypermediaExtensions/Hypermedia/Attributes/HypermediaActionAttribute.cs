using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaActionAttribute : Attribute
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }
}
