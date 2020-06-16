using System;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Channels;

namespace Microsoft.Extensions.Logging
{
    public class YadlLoggerProvider : ILoggerProvider
    {
        private readonly YadlProviderOptions _yadlLoggerOptions;
        private readonly YadlPubSub _yadlPubSub;

        public YadlLoggerProvider(YadlPubSub yadlPubSub, IOptions<YadlProviderOptions> yadlLoggerOptions) : this(yadlPubSub, yadlLoggerOptions.Value)
        {
        }

        public YadlLoggerProvider(YadlPubSub yadlPubSub, YadlProviderOptions yadlLoggerOptions = null)
        {
            _yadlLoggerOptions = yadlLoggerOptions;
            _yadlPubSub = yadlPubSub;

            if (_yadlLoggerOptions == null)
            {
                throw new ArgumentNullException(nameof(_yadlLoggerOptions));
            }
            
            if (_yadlLoggerOptions.Capacity <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_yadlLoggerOptions.Capacity), $"{nameof(_yadlLoggerOptions)} must be a positive number.");
            }
        }
        
        public void Dispose()
        {
            throw new System.NotImplementedException();
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new YadlLogger(_yadlLoggerOptions, _yadlPubSub);
        }
    }
}