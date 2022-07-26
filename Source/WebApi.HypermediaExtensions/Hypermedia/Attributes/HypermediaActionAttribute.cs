using System;

namespace RESTyard.WebApi.Extensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaActionAttribute : Attribute
    {
        public string Name { get; set; }
        public string Title { get; set; }
    }
}
