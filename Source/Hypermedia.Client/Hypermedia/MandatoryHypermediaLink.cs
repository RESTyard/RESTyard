using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bluehands.Hypermedia.Client.Resolver;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public class MandatoryHypermediaLink<T> : HypermediaLink<T>
        where T : HypermediaClientObject
    {

    }

    public class HypermediaLink<T> : IHypermediaLink
        where T : HypermediaClientObject
    {
        public List<string> Relations { get; set; }

        public Uri Uri { get; set; } = null;
    }

    public interface IHypermediaLink
    {
        Uri Uri { get; set; }

        List<string> Relations { get; set; }
    }
}