﻿using System;

namespace RESTyard.Client.Hypermedia.Attributes
{
    // client should not fill this property
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class ClientIgnoreHypermediaPropertyAttribute : Attribute
    {
    }
}
