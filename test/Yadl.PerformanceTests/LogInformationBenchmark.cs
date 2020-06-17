using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Yadl.PerformanceTests
{
    [MemoryDiagnoser]
    public class LogInformationBenchmark
    {
        private StandardLogging _sl;
        private YadlLogging _yl;

        [GlobalSetup]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => { builder.AddFilter("LoggingBenchmarks", LogLevel.Information); });

            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("TEST");
            _sl = new StandardLogging(logger);
            _yl = new YadlLogging(logger);
        }

        [Benchmark(Baseline = true)]
        public void LogInformation() => _sl.LogInformation();

        [Benchmark]
        public void LogInformationHighPerf() => _yl.LogInformationHighPerf();

        [Benchmark]
        public void LogInformationWithEventId() => _sl.LogInformationWithEventId();

        [Benchmark]
        public void LogInformationHighPerfEventId() => _yl.LogInformationHighPerfEventId();
        
        [Benchmark]
        public void LogInformationWithException() => _sl.LogInformationWithException();

        [Benchmark]
        public void LogInformationHighPerfException() => _yl.LogInformationHighPerfException();
        
        [Benchmark]
        public void LogInformationFull() => _sl.LogInformationFull();

        [Benchmark]
        public void LogInformationHighPerfFull() => _yl.LogInformationHighPerfFull();
    }

    internal class StandardLogging
    {
        private readonly ILogger _logger;
        public StandardLogging(ILogger logger) => _logger = logger;
        public void LogInformation() => _logger.LogInformation("Message from standard logging");

        public void LogInformationWithEventId() =>
            _logger.LogInformation(new EventId(1, "EventId"), "Message from standard logging");

        public void LogInformationWithException() =>
            _logger.LogInformation(new Exception("Exception"), "Message from standard logging");
        
        public void LogInformationFull() => _logger.LogInformation(new EventId(1, "EventId"),
            new Exception("Exception"), "Message from standard logging");
    }

    internal class YadlLogging
    {
        private readonly ILogger _logger;
        public YadlLogging(ILogger logger) => _logger = logger;
        public void LogInformationHighPerf() => _logger.LogInformationHighPerf("Message from high performance logging");

        public void LogInformationHighPerfEventId() =>
            _logger.LogInformationHighPerf(new EventId(1, "EventId"), "Message from high performance logging");

        public void LogInformationHighPerfException() =>
            _logger.LogInformationHighPerf(new Exception("Exception"), "Message from high performance logging");

        public void LogInformationHighPerfFull() => _logger.LogInformationHighPerf(new EventId(1, "EventId"),
            new Exception("Exception"), "Message from high performance logging");
    }
}