﻿using System;

namespace RESTyard.WebApi.Extensions.Hypermedia.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class HypermediaPropertyAttribute : Attribute
    {
        public string Name { get; set; }
    }
}
