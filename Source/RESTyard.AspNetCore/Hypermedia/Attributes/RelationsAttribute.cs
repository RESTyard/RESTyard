using System;
using System.Collections.Generic;

namespace RESTyard.AspNetCore.Hypermedia.Attributes;

[AttributeUsage(AttributeTargets.Property)]
public sealed class RelationsAttribute : Attribute
{
    public string[] Rel { get; }

    public RelationsAttribute(string[] rel)
    {
        Rel = rel;
    }
}