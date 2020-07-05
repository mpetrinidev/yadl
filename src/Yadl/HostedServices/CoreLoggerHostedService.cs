using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.HostedServices
{
    public class CoreLoggerHostedService : BackgroundService
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

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await foreach (var message in ReadAllAsync(stoppingToken))
            {
                if (_processor.Messages.Count == _options.BatchSize)
                {
                    //TODO Move elements to tmp list
                    var tmpMsg = _processor.Messages.ToList();

                    //TODO Clear _processor.Messages
                    _processor.Messages.Clear();
                    _processor.Messages.Add(message);

                    //TODO Insert batch
                    using var bcp = new SqlBulkCopy(_options.ConnectionString)
                    {
                        DestinationTableName = _options.TableDestination,
                        BatchSize = _options.BatchSize
                    };
                    using var reader = ObjectReader.Create(tmpMsg,
                        "ID",
                        "ADDDT",
                        "NIVEL",
                        "NIVEL_DESCRIPCION",
                        "PAQUETE",
                        "EP_ORIGEN",
                        "EP_DESTINO",
                        "COD_RESP",
                        "DESCRIPCION",
                        "DATOS", "TIPO_OBJ", "ID_OBJ", "ID_OBJ_HASH", "SEC_CLIENT", "SEC_BANCO", "ENDDT", "DIFFT",
                        "INFO_ADICIONAL"
                    );
                    
                    await bcp.WriteToServerAsync(reader, stoppingToken);
                }
                else
                {
                    _processor.Messages.Add(message);
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