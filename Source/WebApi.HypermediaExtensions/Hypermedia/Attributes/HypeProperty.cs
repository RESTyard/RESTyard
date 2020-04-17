using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypeProperty : HypePrimary
    {
        public string Name { get; set; }
    }
}
