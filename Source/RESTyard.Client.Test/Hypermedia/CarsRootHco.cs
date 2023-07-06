using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Client.Hypermedia.Commands;
using RESTyard.Relations;

namespace RESTyard.Client.Test.Hypermedia
{
    [HypermediaClientObject("CustomersRoot")]
    public class CarsRootHco : HypermediaClientObject
    {
        [Mandatory]
        [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<CustomersRootHco> Self { get; set; }

        [Mandatory]
        [HypermediaCommand("UploadCarImage")]
        public IHypermediaClientFileUploadAction UploadCarImage { get; set; }
    }
}