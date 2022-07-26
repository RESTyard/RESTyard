using System;
using System.Collections.Generic;
using RESTyard.Client.Hypermedia;
using RESTyard.Client.Hypermedia.Attributes;

namespace Benchmarking.Hcos
{
    public abstract class ReportingHco : HypermediaClientObject
    {
        public DateTimeOffset FromDate { get; set; }

        public DateTimeOffset ToDate { get; set; }

        public int TotalScore { get; set; }

        public decimal PercentageScoreChange { get; set; }

        public int TotalActivityPoints { get; set; }

        public decimal AverageActivityPointsPerDay { get; set; }

        [HypermediaRelations(DefaultHypermediaRelations.EmbeddedEntities.Item)]
        public List<ActivityReportHco> Activities { get; set; }

        [HypermediaRelations(HypermediaRelations.Highlights)]
        public List<HighlightReportHco> Highlights { get; set; }

        [HypermediaRelations(HypermediaRelations.EventCaseReports)]
        public List<EventCaseReportHco> EventCases { get; set; }

        [HypermediaRelations(HypermediaRelations.CategoryReports)]
        public List<CategoryReportHco> CategoryReports { get; set; }

        [HypermediaRelations(HypermediaRelations.Scores)]
        public List<ScoreHco> Scores { get; set; }
    }

    public abstract class ReportingHco<TNavigationCommand, TProgressHco>
        : ReportingHco, INavigationHco<TProgressHco>
        where TNavigationCommand : HypermediaNavigationCommand<TProgressHco>
        where TProgressHco : ProgressHco
    {
        [HypermediaRelations(DefaultHypermediaRelations.Queries.Previous)]
        public TNavigationCommand Previous { get; set; }

        IHypermediaNavigationCommand<TProgressHco> INavigationHco<TProgressHco>.Previous => this.Previous;

        [HypermediaRelations(DefaultHypermediaRelations.Queries.Next)]
        public TNavigationCommand Next { get; set; }

        IHypermediaNavigationCommand<TProgressHco> INavigationHco<TProgressHco>.Next => this.Next;
    }
}