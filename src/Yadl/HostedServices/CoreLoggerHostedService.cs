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

        public CoreLoggerHostedService(IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options, ISqlServerBulk sqlServerBulk) : this(processor, options.Value, sqlServerBulk)
        {
        }

        public CoreLoggerHostedService(IYadlProcessor processor,
            YadlLoggerOptions options, ISqlServerBulk sqlServerBulk)
        {
            _processor = processor;
            _options = options;
            _sqlServerBulk = sqlServerBulk;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _processor.ChannelReader.ReadAsync(stoppingToken);
                if (message == null) continue;

                _processor.Messages.Add(message);
                if (_processor.Messages.Count != _options.BatchSize) continue;

                var tmpMsg = _processor.Messages.ToList();
                _processor.Messages.Clear();

                await _sqlServerBulk.ExecuteAsync(tmpMsg, stoppingToken);
            }
        }
    }
}