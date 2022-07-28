using System;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Relations;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("ActivityReport")]
    public class ActivityReportHco : HypermediaClientObject
    {
        public DateTimeOffset Date { get; set; }

        public Guid EventCaseId { get; set; }

        public string EventCaseTitle { get; set; }

        public int CategoryId { get; set; }

        public int Points { get; set; }

        public string Comment { get; set; }

        [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<ActivityReportHco> Self { get; set; }
    }
}