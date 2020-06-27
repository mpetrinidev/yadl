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
    
        [Fact]
        public void LogInformationBenchmark()
        {
            BenchmarkRunner.Run<LogInformationBenchmark>();
        }
        
        [Fact]
        public void LogWarningBenchmark()
        {
            BenchmarkRunner.Run<LogWarningBenchmark>();
        }
    }
}