using System;

namespace WebApi.HypermediaExtensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaPropertyAttribute : LeadingHypermediaAttribute
    {
        public string Name { get; set; }
    }
}
