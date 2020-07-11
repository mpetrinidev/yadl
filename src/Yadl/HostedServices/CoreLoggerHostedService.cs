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
                _processor.Messages.Add(message);
                if (_processor.Messages.Count != _options.BatchSize) continue;

                var tmpMsg = _processor.Messages.ToList();
                _processor.Messages.Clear();

                try
                {
                    using var bcp = new SqlBulkCopy(_options.ConnectionString)
                    {
                        DestinationTableName = _options.TableDestination,
                        BatchSize = _options.BatchSize
                    };
                    await using var reader = ObjectReader.Create(tmpMsg, "Id", 
                        "Message",
                        "Level",
                        "LevelDescription",
                        "TimeStamp",
                        "ExtraFields");

                    await bcp.WriteToServerAsync(reader, stoppingToken);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
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