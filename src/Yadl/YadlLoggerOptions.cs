using System;
using System.Collections.Generic;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;

namespace Yadl
{
    public class YadlLoggerOptions
    {
        public bool IncludeScopes { get; set; } = true;
        public Func<string, LogLevel, bool> Filter { get; set; }
        public Dictionary<string,object> GlobalFields { get; set; } = new Dictionary<string, object>();
        public bool IncludeMessageTemplates { get; set; }

        public int BatchSize { get; set; }
        public string ConnectionString { get; set; }
        public string TableDestination { get; set; }
        public string ProjectPackage { get; set; }
        public int BatchPeriod { get; set; }

        public BoundedChannelFullMode ChannelFullMode { get; set; } = BoundedChannelFullMode.Wait;
        
        public double TimerInsert { get; set; }
    }
}