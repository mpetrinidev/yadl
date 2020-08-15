using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class TimedHostedService : BackgroundService
    {
        private readonly IYadlProcessor _processor;
        private readonly YadlLoggerOptions _options;
        private readonly ISqlServerBulk _sqlServerBulk;

        private readonly object _memberLock;

        public TimedHostedService(IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options, ISqlServerBulk sqlServerBulk) : this(processor, options.Value,
            sqlServerBulk)
        {
        }

        public TimedHostedService(IYadlProcessor processor,
            YadlLoggerOptions options, ISqlServerBulk sqlServerBulk)
        {
            _processor = processor;
            _options = options;
            _sqlServerBulk = sqlServerBulk;

            _memberLock = new object();
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(_options.BatchPeriod, stoppingToken);

                List<YadlMessage> messages;
                lock (_memberLock)
                {
                    if (_processor.ChannelReader.TryRead(out var message))
                        _processor.Messages.Add(message);

                    messages = _processor.Messages.ToList();
                    _processor.Messages.Clear();
                }

                if (messages.Count > 0)
                    await _sqlServerBulk.ExecuteAsync(messages, stoppingToken);
            }
        }
    }
}