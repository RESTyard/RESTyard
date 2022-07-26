using System;

namespace RESTyard.Client.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class MandatoryAttribute : Attribute
    {
    }
}