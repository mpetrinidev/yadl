using BenchmarkDotNet.Running;
using Xunit;

namespace Yadl.PerformanceTests
{
    public class SummaryTests
    {
        [Fact]
        public void JsonBenchmark()
        {
            BenchmarkRunner.Run<JsonBenchmark>();
        }
    }
}