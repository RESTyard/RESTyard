using System;
using System.Collections.Generic;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public interface IHypermediaLink : ICanResolve
    {
        Uri Uri { get; set; }
        List<string> Relations { get; set; }
    }
}