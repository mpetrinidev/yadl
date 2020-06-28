using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class CoreLoggerHostedService : IHostedService
    {
        private readonly ILogger<CoreLoggerHostedService> _logger;
        private readonly IYadlProcessor _processor;
        private readonly YadlLoggerOptions _options;

        public CoreLoggerHostedService(ILogger<CoreLoggerHostedService> logger, IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options) : this(logger, processor, options.Value)
        {
        }

        public CoreLoggerHostedService(ILogger<CoreLoggerHostedService> logger, IYadlProcessor processor,
            YadlLoggerOptions options)
        {
            _logger = logger;
            _processor = processor;
            _options = options;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            await foreach (var message in ReadAllAsync(cancellationToken))
            {
                if (_processor.Messages.Count == _options.BatchSize)
                {
                    //TODO Move elements to tmp list
                    var tmpMsg = new YadlMessage[] { };
                    _processor.Messages.CopyTo(tmpMsg, 0);

                    //TODO Clear _processor.Messages
                    _processor.Messages.Clear();

                    //TODO Insert batch
                }

                _processor.Messages.Add(message);
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Core logger hosted service is stopping");
            return Task.CompletedTask;
        }

        private async IAsyncEnumerable<YadlMessage> ReadAllAsync(
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            while (await _processor.ChannelReader.WaitToReadAsync(cancellationToken).ConfigureAwait(false))
            {
                while (_processor.ChannelReader.TryRead(out YadlMessage item))
                {
                    yield return item;
                }
            }
        }
    }
}