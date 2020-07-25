using System;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;
using Yadl.Json;

namespace Microsoft.Extensions.Logging
{
    public class YadlLogger : ILogger
    {
        private readonly string _name;
        private readonly YadlLoggerOptions _options;
        private readonly IYadlProcessor _processor;
        private readonly JsonSerializerOptions _jsonSerializerOptions;

        public YadlLogger(string name, IOptions<YadlLoggerOptions> options, IYadlProcessor processor) : this(name,
            options.Value,
            processor)
        {
        }

        public YadlLogger(string name, YadlLoggerOptions options, IYadlProcessor processor)
        {
            _name = name;
            _options = options;
            _processor = processor;
            _jsonSerializerOptions = new JsonSerializerOptions();
            _jsonSerializerOptions.Converters.Add(new DictionaryConverter());
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

            //Assume always gonna be true because unbounded channel
            _ = _processor.ChannelWriter.TryWrite(message);
        }

        private void CompleteMessage(YadlMessage message)
        {
            if (_options.GlobalFields.Count > 0)
            {
                ProcessFields(message, _options.GlobalFields);
            }

            var addFields = GetScopeAdditionalFields().ToList();
            if (addFields.Count > 0)
            {
                ProcessFields(message, addFields);
            }
        }

        private IEnumerable<KeyValuePair<string, object>> GetScopeAdditionalFields()
        {
            var additionalFields = Enumerable.Empty<KeyValuePair<string, object>>();

            if (_options.IncludeScopes == false)
            {
                return additionalFields;
            }

            var scope = YadlScope.Current;
            while (scope != null)
            {
                additionalFields = additionalFields.Concat(scope.AdditionalFields);
                scope = scope.Parent;
            }

            return additionalFields;
        }

        private void ProcessFields(YadlMessage message, IEnumerable<KeyValuePair<string, object>> fields)
        {
            if (fields == null) return;
            var actualJson = string.IsNullOrEmpty(message.ExtraFields) ? "{}" : message.ExtraFields;

            var fieldsAsJson =
                JsonSerializer.Serialize(fields.ToDictionary(k => k.Key, v => v.Value), _jsonSerializerOptions);
            message.ExtraFields = JsonExtensions.Merge(actualJson, fieldsAsJson);
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && (_options.Filter == null || _options.Filter(_name, logLevel));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return state switch
            {
                IEnumerable<KeyValuePair<string, object>> fields => PushValidFields(fields),
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
                _ => new NoopDisposable()
            } ?? new NoopDisposable();

            IDisposable PushValidFields(IEnumerable<KeyValuePair<string, object>> fields)
            {
                var dic = fields.Where(p => _options.AllowedKeys.Contains(p.Key))
                    .ToDictionary(d => d.Key, d => d.Value);

                return dic.Count == 0 ? new NoopDisposable() : YadlScope.Push(dic);
            }

            IDisposable ConvertTuple((string, object) field) => _options.AllowedKeys.Contains(field.Item1)
                ? YadlScope.Push(new[]
                {
                    new KeyValuePair<string, object>(field.Item1, field.Item2)
                })
                : new NoopDisposable();
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
                _ => string.Empty
            } ?? string.Empty;
        }

        private class NoopDisposable : IDisposable
        {
            public void Dispose()
            {
            }
        }
    }
}