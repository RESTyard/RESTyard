using HypermediaClient.Hypermedia;
using HypermediaClient.Hypermedia.Attributes;

namespace HypermediaClient.Test.Hypermedia
{
    [HypermediaClientObject(Classes = new[] { "EntryPoint" })]
    public class EntryPointHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new [] {"Self"})]
        public MandatoryHypermediaLink<EntryPointHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations(new[] { "CustomersRoot" })]
        public MandatoryHypermediaLink<CustomersRootHco> Customers { get; set; }
    }
}
