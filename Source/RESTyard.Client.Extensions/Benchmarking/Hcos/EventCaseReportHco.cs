using System;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;
using RESTyard.Relations;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("EventCaseReport")]
    public class EventCaseReportHco : HypermediaClientObject
    {
        public new string Title { get; set; }

        public int CategoryId { get; set; }

        public Guid EventCaseId { get; set; }

        public int TotalScore { get; set; }

        public int TimesAccomplished { get; set; }

        [HypermediaRelations(new[] { DefaultHypermediaRelations.Self })]
        public MandatoryHypermediaLink<EventCaseReportHco> Self { get; set; }
    }
}