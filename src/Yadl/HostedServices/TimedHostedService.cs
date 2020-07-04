using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class TimedHostedService : BackgroundService
    {
        private readonly ILogger<TimedHostedService> _logger;
        private readonly IYadlProcessor _processor;
        private readonly YadlLoggerOptions _options;

        public TimedHostedService(ILogger<TimedHostedService> logger, IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options) : this(logger, processor, options.Value)
        {
        }

        public TimedHostedService(ILogger<TimedHostedService> logger, IYadlProcessor processor,
            YadlLoggerOptions options)
        {
            _logger = logger;
            _processor = processor;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_options.BatchSize == _processor.Messages.Count)
                {
                    //TODO Move elements to another list and clear _processor.Messages
                    //TODO Clear _processor.Messages
                    //TODO Insert batch
                }

                await Task.Delay(_options.BatchPeriod);
            }
        }
    }
}