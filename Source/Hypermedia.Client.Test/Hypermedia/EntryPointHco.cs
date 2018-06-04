using Hypermedia.Relations;

using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Attributes;

namespace Hypermedia.Client.Test.Hypermedia
{


    [HypermediaClientObject(Classes = new[] { "EntryPoint" })]
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
