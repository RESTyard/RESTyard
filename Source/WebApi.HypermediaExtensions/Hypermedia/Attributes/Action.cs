using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Action : Primary
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }
}
