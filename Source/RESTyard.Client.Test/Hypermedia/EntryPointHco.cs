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
        [HypermediaRelations(DefaultHypermediaRelations.Self)]
        public MandatoryHypermediaLink<EntryPointHco> Self { get; set; }

        [Mandatory]
        [HypermediaRelations("CustomersRoot")]
        public MandatoryHypermediaLink<CustomersRootHco> Customers { get; set; }
        
        [Mandatory]
        [HypermediaRelations("CarsRoot")]
        public MandatoryHypermediaLink<CarsRootHco> Cars { get; set; }
    }
}
