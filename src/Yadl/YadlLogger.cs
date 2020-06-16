using System;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using Yadl;
using Yadl.Channels;

namespace Microsoft.Extensions.Logging
{
    public class YadlLogger : ILogger
    {
        private readonly YadlProviderOptions _yadlProviderOptions;
        private readonly ChannelWriter<string> _channelWriter;

        public YadlLogger(YadlProviderOptions yadlProviderOptions, YadlPubSub yadlPubSub)
        {
            _yadlProviderOptions = yadlProviderOptions;
            _channelWriter = yadlPubSub.Channel.Writer;
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            if (!_channelWriter.TryWrite(formatter(state, exception)))
            {
                return;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel == LogLevel.Information;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            throw new NotImplementedException();
        }
    }
}