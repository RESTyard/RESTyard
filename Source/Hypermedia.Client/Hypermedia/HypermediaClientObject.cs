using System.Collections.Generic;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;

namespace Bluehands.Hypermedia.Client.Hypermedia
{
    public abstract class HypermediaClientObject
    {
        private static readonly List<string>  emptyRelation = new List<string>();

        protected HypermediaClientObject()
        {
        }

        [ClientIgnoreHypermediaProperty]
        public List<string> Relations { get; set; } = emptyRelation;

        [ClientIgnoreHypermediaProperty]
        public string Title { get; set; } = string.Empty;
    }
}
