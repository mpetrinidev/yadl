using System;
using Microsoft.Extensions.Options;
using Yadl;

namespace Microsoft.Extensions.Logging
{
    public class YadlLoggerProvider : ILoggerProvider
    {
        private readonly YadlProviderOptions _yadlLoggerOptions;

        public YadlLoggerProvider(IOptions<YadlProviderOptions> yadlLoggerOptions) : this(yadlLoggerOptions.Value)
        {
        }

        public YadlLoggerProvider(YadlProviderOptions yadlLoggerOptions = null)
        {
            _yadlLoggerOptions = yadlLoggerOptions;

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
            return new YadlLogger(_yadlLoggerOptions);
        }
    }
}