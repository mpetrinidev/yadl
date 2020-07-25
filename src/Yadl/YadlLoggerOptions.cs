using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Yadl.Json;

namespace Yadl
{
    public class YadlLoggerOptions
    {
        public bool IncludeScopes { get; set; } = true;
        public Func<string, LogLevel, bool>? Filter { get; set; }
        public IDictionary<string, object> GlobalFields { get; set; } = new Dictionary<string, object>();
        public ICollection<string> AllowedKeys { get; } = new List<string>();

        public JsonSerializerOptions JsonSerializerOptions { get; set; } = new JsonSerializerOptions
        {
            IgnoreNullValues = false,
            MaxDepth = 2,
            Converters = {new DictionaryConverter()}
        };

        public int BatchSize { get; set; }
        public string? ConnectionString { get; set; }
        public string? TableDestination { get; set; }
        public int BatchPeriod { get; set; }

        public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;
    }
}