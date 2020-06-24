using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;

namespace Microsoft.Extensions.Logging
{
    public class YadlLogger : ILogger
    {
        private readonly YadlLoggerOptions _options;
        private readonly IYadlProcessor _processor;

        private readonly IExternalScopeProvider _scopeProvider;

        public YadlLogger(IOptions<YadlLoggerOptions> options, IYadlProcessor processor,
            IExternalScopeProvider scopeProvider) : this(options.Value,
            processor, scopeProvider)
        {
        }

        public YadlLogger(YadlLoggerOptions options, IYadlProcessor processor, IExternalScopeProvider scopeProvider)
        {
            _options = options;
            _processor = processor;
            _scopeProvider = scopeProvider;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception,
            Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            var message = new YadlMessage
            {
                Descripcion = formatter(state, exception)
            };

            var allFields = _options.GlobalFields;
            _scopeProvider?.ForEachScope((scope, mes) =>
            {
                if (scope is IEnumerable<KeyValuePair<string, object>> pairs)
                {
                    foreach (var pair in pairs)
                    {
                        //TODO map dict to object?
                    }
                }
            }, message);

            if (!_processor.ChannelWriter.TryWrite(message))
            {
                return;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public IDisposable BeginScope<TState>(TState state) => _scopeProvider?.Push(state);

        // public IDisposable BeginScope<TState>(TState state)
        // {
        //     return state switch
        //     {
        //         IEnumerable<KeyValuePair<string, object>> fields => YadlLogScope.Push(fields),
        //         ValueTuple<string, string> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, short> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, ushort> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, int> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, uint> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, long> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, ulong> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, float> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, double> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, decimal> field => AppendTupleToDictionary(field),
        //         ValueTuple<string, object> field => AppendTupleToDictionary(field),
        //         _ => new NoopDisposable()
        //     };
        //
        //     IDisposable AppendTupleToDictionary((string, object) field) => YadlLogScope.Push(new[]
        //     {
        //         new KeyValuePair<string, object>(field.Item1, field.Item2)
        //     });
        // }
    }
}