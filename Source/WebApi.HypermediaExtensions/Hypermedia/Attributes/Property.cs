using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Property : Primary
    {
        public string Name { get; set; }
    }
}
