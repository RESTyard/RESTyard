namespace Hypermedia.Client.Hypermedia.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaClientObjectAttribute : Attribute
    {
        public string[] Classes { get; set; }
    }
}
