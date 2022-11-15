using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Util.Relations;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("CategoryReport")]
    public class CategoryReportHco : HypermediaClientObject
    {
        public int CategoryId { get; set; }

        public int Score { get; set; }

        public int NumberOfTiles { get; set; }

        public int TimesAccomplished { get; set; }

        public decimal AveragePointsPerDay { get; set; }

        [HypermediaRelations(DefaultHypermediaRelations.Self)]
        public MandatoryHypermediaLink<CategoryReportHco> Self { get; set; }
    }
}