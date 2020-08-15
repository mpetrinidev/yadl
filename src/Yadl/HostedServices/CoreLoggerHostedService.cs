using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class CoreLoggerHostedService : BackgroundService
    {
        private readonly IYadlProcessor _processor;
        private readonly YadlLoggerOptions _options;
        private readonly ISqlServerBulk _sqlServerBulk;
        private readonly object _memberLock;

        public CoreLoggerHostedService(IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options, ISqlServerBulk sqlServerBulk) : this(processor, options.Value,
            sqlServerBulk)
        {
        }

        public CoreLoggerHostedService(IYadlProcessor processor,
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
                var message = await _processor.ChannelReader.ReadAsync(stoppingToken);
                if (message == null) continue;

                List<YadlMessage> messages;
                lock (_memberLock)
                {
                    _processor.Messages.Add(message);
                    if (_processor.Messages.Count != _options.BatchSize) continue;

                    messages = _processor.Messages.ToList();
                    _processor.Messages.Clear();
                }

                await _sqlServerBulk.ExecuteAsync(messages, stoppingToken);
            }
        }
    }
}