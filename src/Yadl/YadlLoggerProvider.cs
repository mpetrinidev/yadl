using System;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;

namespace Microsoft.Extensions.Logging
{
    [ProviderAlias("Yadl")]
    public class YadlLoggerProvider : ILoggerProvider, ISupportExternalScope
    {
        private readonly YadlLoggerOptions _options;
        private readonly IYadlProcessor _processor;
        private IExternalScopeProvider? _scopeProvider;

        public YadlLoggerProvider(IYadlProcessor processor, IOptions<YadlLoggerOptions> options) : this(processor,
            options.Value)
        {
        }

        public YadlLoggerProvider(IYadlProcessor processor, YadlLoggerOptions options)
        {
            _options = options;
            _processor = processor;

            if (_options == null)
            {
                throw new ArgumentNullException(nameof(_options));
            }

            if (_processor == null)
            {
                throw new ArgumentNullException(nameof(_processor));
            }

            if (_options.BatchSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(_options.BatchSize),
                    $"{nameof(_options.BatchSize)} must be a positive number.");
            }

            if (string.IsNullOrEmpty(_options.ConnectionString))
            {
                throw new ArgumentNullException(nameof(_options.ConnectionString),
                    $"{nameof(_options.ConnectionString)} cannot be null or empty.");
            }

            if (string.IsNullOrEmpty(_options.TableDestination))
            {
                throw new ArgumentNullException(nameof(_options.TableDestination),
                    $"{nameof(_options.TableDestination)} cannot be null or empty.");
            }
        }

        public void Dispose()
        {
        }

        public ILogger CreateLogger(string categoryName)
        {
            return new YadlLogger(categoryName, _options, _processor)
            {
                ExternalScopeProvider = _scopeProvider
            };
        }

        void ISupportExternalScope.SetScopeProvider(IExternalScopeProvider scopeProvider)
        {
            _scopeProvider = scopeProvider;
        }
    }
}