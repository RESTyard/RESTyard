using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaObjectAttribute : Primary
    {
        public string Title { get; set; }
        public string[] Classes { get; set; }
    }
}
