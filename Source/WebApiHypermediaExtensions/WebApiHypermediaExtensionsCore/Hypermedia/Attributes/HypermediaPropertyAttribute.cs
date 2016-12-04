using System;

namespace WebApiHypermediaExtensionsCore.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class HypermediaPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
