using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;
using Yadl.Common;
using Yadl.Json;

namespace Microsoft.Extensions.Logging
{
    public class YadlLogger : ILogger
    {
        private readonly string _name;
        private readonly YadlLoggerOptions _options;
        private readonly IYadlProcessor _processor;

        private readonly IExternalScopeProvider _scopeProvider;

        public YadlLogger(string name, IOptions<YadlLoggerOptions> options, IYadlProcessor processor,
            IExternalScopeProvider scopeProvider) : this(name, options.Value,
            processor, scopeProvider)
        {
        }

        public YadlLogger(string name, YadlLoggerOptions options, IYadlProcessor processor,
            IExternalScopeProvider scopeProvider)
        {
            _name = name;
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
                Level = (int) logLevel,
                LevelDescription = GetLogDescription(logLevel),
                Message = formatter(state, exception),
                TimeStamp = DateTimeOffset.Now
            };

            CompleteMessage(message);

            if (!_processor.ChannelWriter.TryWrite(message))
            {
                return;
            }
        }

        private void CompleteMessage(YadlMessage message)
        {
            ProcessFields(message, _options.GlobalFields.ToList(), true);
            _scopeProvider?.ForEachScope(
                (scope, mes) =>
                {
                    ProcessFields(message, ((IEnumerable<KeyValuePair<string, object>>) scope).ToList());
                }, message);
        }

        private static void ProcessFields(YadlMessage message, IEnumerable<KeyValuePair<string, object>> fields,
            bool isGlobalFields = false)
        {
            if (fields == null) return;

            var fieldsToDic = fields.ToDictionary(x => x.Key, x => x.Value);

            var firstJson = string.IsNullOrEmpty(message.ExtraFields) ? "{}" : message.ExtraFields;

            var serializerOptions = new JsonSerializerOptions();
            var dicConverter = new DictionaryConverter();
            serializerOptions.Converters.Add(dicConverter);

            var secondJson = JsonSerializer.Serialize(fieldsToDic, serializerOptions);

            message.ExtraFields = JsonExtensions.Merge(firstJson, secondJson);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && (_options.Filter == null || _options.Filter(_name, logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return state switch
            {
                IEnumerable<KeyValuePair<string, object>> fields => _scopeProvider?.Push(fields),
                ValueTuple<string, string> field => ConvertTuple(field),
                ValueTuple<string, short> field => ConvertTuple(field),
                ValueTuple<string, ushort> field => ConvertTuple(field),
                ValueTuple<string, int> field => ConvertTuple(field),
                ValueTuple<string, uint> field => ConvertTuple(field),
                ValueTuple<string, long> field => ConvertTuple(field),
                ValueTuple<string, ulong> field => ConvertTuple(field),
                ValueTuple<string, float> field => ConvertTuple(field),
                ValueTuple<string, double> field => ConvertTuple(field),
                ValueTuple<string, decimal> field => ConvertTuple(field),
                ValueTuple<string, object> field => ConvertTuple(field),
                _ => NullScope.Instance
            };

            IDisposable ConvertTuple((string, object) field) => _scopeProvider?.Push(new[]
            {
                new KeyValuePair<string, object>(field.Item1, field.Item2)
            });
        }

        private string GetLogDescription(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => "Trace",
                LogLevel.Debug => "Debug",
                LogLevel.Information => "Information",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                LogLevel.None => "None",
                _ => null
            };
        }
    }
}