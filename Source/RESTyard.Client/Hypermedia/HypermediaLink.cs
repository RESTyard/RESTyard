using System;
using System.Collections.Generic;
using RESTyard.Client.Resolver;

namespace RESTyard.Client.Hypermedia
{
    public class HypermediaLink<T> : IHypermediaLink where T : HypermediaClientObject
    {
        public IReadOnlyList<string> Relations { get; set; } = Array.Empty<string>();

        public Uri? Uri { get; set; } = null;

        public IHypermediaResolver Resolver { get; set; }
    }
}