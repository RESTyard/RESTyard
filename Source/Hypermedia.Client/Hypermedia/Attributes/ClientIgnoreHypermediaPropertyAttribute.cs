namespace Hypermedia.Client.Hypermedia.Attributes
{
    using System;

    // client should not fill this property
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ClientIgnoreHypermediaPropertyAttribute : Attribute
    {
    }
}
