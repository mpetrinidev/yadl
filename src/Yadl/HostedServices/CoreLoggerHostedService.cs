using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class CoreLoggerHostedService : BackgroundService
    {
        private readonly IYadlProcessor _processor;
        private readonly YadlLoggerOptions _options;

        public CoreLoggerHostedService(IYadlProcessor processor,
            IOptions<YadlLoggerOptions> options) : this(processor, options.Value)
        {
        }

        public CoreLoggerHostedService(IYadlProcessor processor,
            YadlLoggerOptions options)
        {
            _processor = processor;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var message in ReadAllAsync(stoppingToken))
            {
                if (_processor.Messages.Count == _options.BatchSize)
                {
                    var tmpMsg = _processor.Messages.ToList();
                    _processor.Messages.Clear();
                    
                    using var bcp = new SqlBulkCopy(_options.ConnectionString)
                    {
                        DestinationTableName = _options.TableDestination,
                        BatchSize = _options.BatchSize
                    };
                    await using var reader = ObjectReader.Create(tmpMsg, "Message",
                        "Level",
                        "LevelDescription",
                        "TimeStamp",
                        "ExtraFields");

                    await bcp.WriteToServerAsync(reader, stoppingToken);
                }

                _processor.Messages.Add(message);
            }
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