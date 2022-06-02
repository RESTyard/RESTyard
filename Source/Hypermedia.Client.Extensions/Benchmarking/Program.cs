using System.Threading.Tasks;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Reports;
using BenchmarkDotNet.Running;

namespace Benchmarking
{
    public class Program
    {
        public static async Task Main()
        {
            var config = DefaultConfig.Instance.WithSummaryStyle(SummaryStyle.Default.WithMaxParameterColumnWidth(100));
            BenchmarkRunner.Run<StringReaderPlusExportAsString>(config: config);
        }
    }
}