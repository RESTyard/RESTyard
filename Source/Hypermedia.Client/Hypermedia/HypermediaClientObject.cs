namespace Hypermedia.Client.Hypermedia
{
    using System.Collections.Generic;

    using global::Hypermedia.Client.Hypermedia.Attributes;

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
