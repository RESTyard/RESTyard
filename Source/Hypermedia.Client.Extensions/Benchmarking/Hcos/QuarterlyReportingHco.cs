using Bluehands.Hypermedia.Client.Hypermedia.Attributes;

namespace Benchmarking.Hcos
{
    [HypermediaClientObject("QuarterlyReporting")]
    public class QuarterlyReportingHco
        : ReportingHco<QuarterlyReportingNavigationCommandHco, QuarterlyReportingProgressHco>
    {
    }

    [HypermediaClientObject("QuarterlyReportingProgress")]
    public class QuarterlyReportingProgressHco
        : ProgressHco<QuarterlyReportingHco>
    {
    }

    [HypermediaClientObject("QuarterlyReportingNavigationCommand")]
    public class QuarterlyReportingNavigationCommandHco
        : HypermediaNavigationCommand<QuarterlyReportingProgressHco>
    {
    }
}