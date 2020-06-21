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
        private Dictionary<string, object> _fields => new Dictionary<string, object>();

        public YadlLogger(IOptions<YadlLoggerOptions> options, IYadlProcessor processor) : this(options.Value, processor)
        {
            
        }
        
        public YadlLogger(YadlLoggerOptions options, IYadlProcessor processor)
        {
            _options = options;
            _processor = processor;
        }
        
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            if (!IsEnabled(logLevel)) return;

            var message = new YadlMessage
            {
                
            };
            
            if (!_processor.ChannelWriter.TryWrite(message))
            {
                return;
            }
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return state switch
            {
                Dictionary<string, object> fields => AppendFieldsToDictionary(fields),
                ValueTuple<string, string> field => AppendTupleToDictionary(field),
                ValueTuple<string, short> field => AppendTupleToDictionary(field),
                ValueTuple<string, ushort> field => AppendTupleToDictionary(field),
                ValueTuple<string, int> field => AppendTupleToDictionary(field),
                ValueTuple<string, uint> field => AppendTupleToDictionary(field),
                ValueTuple<string, long> field => AppendTupleToDictionary(field),
                ValueTuple<string, ulong> field => AppendTupleToDictionary(field),
                ValueTuple<string, float> field => AppendTupleToDictionary(field),
                ValueTuple<string, double> field => AppendTupleToDictionary(field),
                ValueTuple<string, decimal> field => AppendTupleToDictionary(field),
                ValueTuple<string, object> field => AppendTupleToDictionary(field),
                _ => new NoopDisposable()
            };

            IDisposable AppendTupleToDictionary((string, object) field)
            {
                _fields.Add(field.Item1, field.Item2);
                return new NoopDisposable();
            }
        }

        private IDisposable AppendFieldsToDictionary(Dictionary<string, object> fields)
        {
            foreach (var field in fields)
            {
                _fields.Add(field.Key, field.Value);
            }
            
            return new NoopDisposable();
        } 
        
        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}