using System;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Relations;

namespace RESTyard.Client.Test.Hypermedia
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
