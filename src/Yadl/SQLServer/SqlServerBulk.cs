using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FastMember;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;
using Yadl.Abstractions;

namespace Yadl.SQLServer
{
    public class SqlServerBulk : ISqlServerBulk
    {
        private readonly YadlLoggerOptions _options;

        public SqlServerBulk(IOptions<YadlLoggerOptions> options) : this(options.Value)
        {
        }

        public SqlServerBulk(YadlLoggerOptions options)
        {
            _options = options;
        }

        public async Task ExecuteAsync(ICollection<YadlMessage> messages, CancellationToken cancellationToken = default)
        {
            using var bcp = new SqlBulkCopy(_options.ConnectionString, _options.SqlBulkCopyOptions)
            {
                DestinationTableName = _options.TableDestination,
                BatchSize = _options.BatchSize,
                BulkCopyTimeout = 0
            };
            await using var reader = ObjectReader.Create(messages, "Id",
                "Message",
                "Level",
                "LevelDescription",
                "TimeStamp",
                "ExtraFields");

            await bcp.WriteToServerAsync(reader, cancellationToken);
        }
    }
}