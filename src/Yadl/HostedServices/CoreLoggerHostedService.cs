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
            while (!stoppingToken.IsCancellationRequested)
            {
                var message = await _processor.ChannelReader.ReadAsync();
                if (message == null) continue;

                _processor.Messages.Add(message);
                if (_processor.Messages.Count != _options.BatchSize) continue;

                var tmpMsg = _processor.Messages.ToList();
                _processor.Messages.Clear();

                using var bcp = new SqlBulkCopy(_options.ConnectionString,
                    SqlBulkCopyOptions.KeepNulls |
                    SqlBulkCopyOptions.UseInternalTransaction)
                {
                    DestinationTableName = _options.TableDestination,
                    BatchSize = _options.BatchSize,
                    BulkCopyTimeout = 0
                };
                await using var reader = ObjectReader.Create(tmpMsg, "Id",
                    "Message",
                    "Level",
                    "LevelDescription",
                    "TimeStamp",
                    "ExtraFields");

                await bcp.WriteToServerAsync(reader, stoppingToken);
            }
        }
    }
}