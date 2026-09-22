using System;

namespace RESTyard.AspNetCore.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaObjectAttribute : Attribute
    {
        public string[]? Classes { get; set; }
    }
}
