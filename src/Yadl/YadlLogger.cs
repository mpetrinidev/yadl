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

            var addFields = GetScopeAdditionalFields();
            if (addFields.Count > 0)
            {
                ProcessFields(message, addFields);
            }
        }

        private IDictionary<string, object> GetScopeAdditionalFields()
        {
            var additionalFields = new Dictionary<string, object>();

            if (_options.IncludeScopes == false)
            {
                return additionalFields;
            }

            var scope = YadlScope.Current;
            while (scope != null)
            {
                additionalFields = additionalFields.Concat(scope.AdditionalFields)
                    .ToDictionary(d => d.Key, d => d.Value);

                scope = scope.Parent;
            }

            return additionalFields;
        }

        private void ProcessFields(YadlMessage message, IDictionary<string, object> fields)
        {
            if (fields == null) return;
            var actualJson = string.IsNullOrEmpty(message.ExtraFields) ? "{}" : message.ExtraFields;

            var fieldsAsJson = JsonSerializer.Serialize(fields, _options.JsonSerializerOptions);

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
                IDictionary<string, object?> fields => PushValidFields(fields),
                ValueTuple<string, string?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, short?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, ushort?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, int?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, uint?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, long?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, ulong?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, float?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, double?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, decimal?> field => ConvertTupleAndPushValidFields(field),
                ValueTuple<string, object?> field => ConvertTupleAndPushValidFields(field),
                _ => new NoopDisposable()
            } ?? new NoopDisposable();

            IDisposable PushValidFields(IDictionary<string, object?> fields)
            {
                var dic = GetAllowedDictionary(fields);
                return dic.Count == 0 ? new NoopDisposable() : YadlScope.Push(dic);
            }

            IDisposable ConvertTupleAndPushValidFields((string, object?) fields) =>
                _options.AllowedKeys.Contains(fields.Item1)
                    ? YadlScope.Push(new Dictionary<string, object?> {{fields.Item1, fields.Item2}})
                    : new NoopDisposable();

            Dictionary<string, object?> GetAllowedDictionary(IDictionary<string, object?> fields) =>
                fields
                    .Where(p => _options.AllowedKeys.Contains(p.Key))
                    .ToDictionary(d => d.Key, d => d.Value);
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