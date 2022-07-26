using System;
using System.Collections.Generic;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Hypermedia
{
    public interface IHypermediaLink
    {
        Uri Uri { get; set; }

        List<string> Relations { get; set; }

        IHypermediaResolver Resolver { get; set; }
    }
}