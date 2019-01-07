using System;

namespace Bluehands.Hypermedia.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HypermediaClientObjectAttribute : Attribute
    {
        public string[] Classes { get; set; }
    }
}
