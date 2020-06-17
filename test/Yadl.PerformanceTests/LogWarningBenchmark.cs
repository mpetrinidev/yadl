using System;
using BenchmarkDotNet.Attributes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Yadl.PerformanceTests
{
    [MemoryDiagnoser]
    public class LogWarningBenchmark
    {
        private StandardWarningLogging _sl;
        private YadlWarningLogging _yl;

        [GlobalSetup]
        public void Setup()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(builder => { builder.AddFilter("LoggingBenchmarks", LogLevel.Information); });

            var loggerFactory = serviceCollection.BuildServiceProvider().GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger("TEST");
            _sl = new StandardWarningLogging(logger);
            _yl = new YadlWarningLogging(logger);
        }

        [Benchmark]
        public void LogWarning() => _sl.LogWarning();

        [Benchmark]
        public void LogWarningHighPerf() => _yl.LogWarningHighPerf();

        [Benchmark]
        public void LogWarningWithEventId() => _sl.LogWarningWithEventId();

        [Benchmark]
        public void LogWarningHighPerfEventId() => _yl.LogWarningHighPerfEventId();
        
        [Benchmark]
        public void LogWarningWithException() => _sl.LogWarningWithException();

        [Benchmark]
        public void LogWarningHighPerfException() => _yl.LogWarningHighPerfException();
        
        [Benchmark]
        public void LogWarningFull() => _sl.LogWarningFull();

        [Benchmark]
        public void LogWarningHighPerfFull() => _yl.LogWarningHighPerfFull();
    }

    internal class StandardWarningLogging
    {
        private readonly ILogger _logger;
        public StandardWarningLogging(ILogger logger) => _logger = logger;
        public void LogWarning() => _logger.LogWarning("Message from standard logging");

        public void LogWarningWithEventId() =>
            _logger.LogWarning(new EventId(1, "EventId"), "Message from standard logging");

        public void LogWarningWithException() =>
            _logger.LogWarning(new Exception("Exception"), "Message from standard logging");
        
        public void LogWarningFull() => _logger.LogWarning(new EventId(1, "EventId"),
            new Exception("Exception"), "Message from standard logging");
    }

    internal class YadlWarningLogging
    {
        private readonly ILogger _logger;
        public YadlWarningLogging(ILogger logger) => _logger = logger;
        public void LogWarningHighPerf() => _logger.LogWarningHighPerf("Message from high performance logging");

        public void LogWarningHighPerfEventId() =>
            _logger.LogWarningHighPerf(new EventId(1, "EventId"), "Message from high performance logging");

        public void LogWarningHighPerfException() =>
            _logger.LogWarningHighPerf(new Exception("Exception"), "Message from high performance logging");

        public void LogWarningHighPerfFull() => _logger.LogWarningHighPerf(new EventId(1, "EventId"),
            new Exception("Exception"), "Message from high performance logging");
    }
}