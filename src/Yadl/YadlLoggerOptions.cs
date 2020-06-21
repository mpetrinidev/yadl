using System.Collections.Generic;
using System.Threading.Channels;

namespace Yadl
{
    public class YadlLoggerOptions
    {
        public bool IncludeScopes { get; set; } = true;
        public Dictionary<string,object> GlobalFields { get; set; } = new Dictionary<string, object>();
        public bool IncludeMessageTemplates { get; set; }

        public int BatchSize { get; set; }
        public string LogCnnStr { get; set; }
        public string TableDestination { get; set; }
        public string ProjectPackage { get; set; }
        public int BatchPeriod { get; set; }
        
        public BoundedChannelFullMode ChannelFullMode { get; set; }
        
        public double TimerInsert { get; set; }
    }
}