using System.Collections.Generic;
using HypermediaClient.Hypermedia.Attributes;

namespace HypermediaClient.Hypermedia
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
