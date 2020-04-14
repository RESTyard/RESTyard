using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaObjectAttribute : LeadingHypermediaAttribute
    {
        public string Title { get; set; }
        public string[] Classes { get; set; }
    }
}
