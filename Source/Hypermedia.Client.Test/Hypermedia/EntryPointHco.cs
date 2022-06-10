using Bluehands.Hypermedia.Client.Hypermedia;
using Bluehands.Hypermedia.Client.Hypermedia.Attributes;
using Bluehands.Hypermedia.Relations;

namespace Bluehands.Hypermedia.Client.Test.Hypermedia
{
    [HypermediaClientObject("Entrypoint")]
    public class EntryPointHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new [] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<EntryPointHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations(new[] { "CustomersRoot" })]
        public MandatoryHypermediaLink<CustomersRootHco> Customers { get; set; }
    }
}
