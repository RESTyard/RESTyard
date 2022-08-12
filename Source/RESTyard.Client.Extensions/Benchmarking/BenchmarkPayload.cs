using System.IO;

namespace Benchmarking
{
    public static class BenchmarkPayload
    {
        public static string GetJsonPayload()
        {
            return File.ReadAllText("payload.json");
        }
    }
}