using System;

namespace RESTyard.AspNetCore.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
