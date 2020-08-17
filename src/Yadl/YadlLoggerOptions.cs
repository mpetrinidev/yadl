using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Yadl.Json;

namespace Yadl
{
    public class YadlLoggerOptions
    {
        public bool IncludeScopes { get; set; } = true;
        public Func<string, LogLevel, bool>? Filter => null;

        public IDictionary<string, object?> GlobalFields { get; set; } = new Dictionary<string, object?>();
        public bool UseAllowedKeys { get; set; } = false;
        public ICollection<string> AllowedKeys { get; } = new List<string>();

        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            IgnoreNullValues = false,
            WriteIndented = false,
            AllowTrailingCommas = false,
            Converters = {new DictionaryConverter()}
        };

        public SqlBulkCopyOptions SqlBulkCopyOptions { get; set; } = SqlBulkCopyOptions.KeepNulls |
                                                                     SqlBulkCopyOptions.UseInternalTransaction;

        public int BatchSize { get; set; }
        public string? ConnectionString { get; set; }
        public string? TableDestination { get; set; }
        public int BatchPeriod { get; set; }

        public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;
    }
}