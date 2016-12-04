using System;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class HypermediaObjectAttribute : Attribute
    {
        public string Title { get; set; }
        public string[] Classes { get; set; }
    }
}
