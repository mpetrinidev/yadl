using System;
using System.Text.Json;
using System.Text.Json.Serialization;
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
                Nivel = GetLogLevel(logLevel),
                NivelDescripcion = GetLogDescription(logLevel),
                Descripcion = formatter(state, exception),
                Paquete = _options.ProjectPackage,
                EndDt = DateTimeOffset.Now
            };

            CompleteMessage(message);

            if (!_processor.ChannelWriter.TryWrite(message))
            {
                return;
            }
        }

        private void CompleteMessage(YadlMessage message)
        {
            ProcessFields(message, _options.GlobalFields, true);
            _scopeProvider?.ForEachScope(
                (scope, mes) =>
                {
                    ProcessFields(message, (IEnumerable<KeyValuePair<string, object>>) scope);
                }, message);
        }

        private static void ProcessFields(YadlMessage message, IEnumerable<KeyValuePair<string, object>> fields,
            bool isGlobalFields = false)
        {
            if (fields == null) return;

            var fieldsAsList = fields.ToList();
            for (var i = fieldsAsList.Count - 1; i >= 0; i--)
            {
                var foundedKey = false;
                switch (fieldsAsList[i].Key.ToLower())
                {
                    case "ep_origen":
                    case "eporigen":
                    case "ip_origen":
                    case "iporigen":
                        message.IpOrigen = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "ep_destino":
                    case "epdestino":
                    case "ip_destino":
                    case "ipdestino":
                        message.IpDestino = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "datos":
                        message.Datos = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "tipo_obj":
                    case "tipoobj":
                        message.TipoObj = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "id_obj":
                    case "idobj":
                        message.IdObj = GetIdObj(fieldsAsList[i].Value);
                        foundedKey = true;

                        break;
                    case "id_obj_hash":
                    case "idobjhash":
                        message.IdObjHash = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "cod_resp":
                    case "codresp":
                        message.CodRespuesta = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "sec_client":
                    case "secclient":
                        message.SecClient = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "sec_banco":
                    case "secbanco":
                    case "sec_bco":
                    case "secbco":
                        message.SecBanco = fieldsAsList[i].Value.ToString();
                        foundedKey = true;

                        break;
                    case "adddt":
                        message.AddDt = GetAddDt(fieldsAsList[i].Value);
                        foundedKey = true;

                        break;
                }

                if (foundedKey && !isGlobalFields) fieldsAsList.RemoveAt(i);
            }

            message.AdditionalInfo = GetAdditionalInfo(message.AdditionalInfo, fieldsAsList.ToDictionary(x => x.Key, x => x.Value));

            long GetIdObj(object value)
            {
                long.TryParse(value.ToString(), out var val);
                return val;
            }

            DateTimeOffset GetAddDt(object value)
            {
                DateTimeOffset.TryParse(value.ToString(), out var val);
                return val;
            }
        }

        private static string GetAdditionalInfo(string messageAdditionalInfo,
            Dictionary<string, object> fieldsAsList)
        {
            var firstJson = string.IsNullOrEmpty(messageAdditionalInfo) ? "{}" : messageAdditionalInfo;
            
            var serializerOptions = new JsonSerializerOptions();
            var dicConverter = new DictionaryConverter();
            serializerOptions.Converters.Add(dicConverter);
            
            var secondJson = JsonSerializer.Serialize(fieldsAsList, serializerOptions);

            var result = JsonExtensions.Merge(firstJson, secondJson);

            return result;
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
                LogLevel.Information => "Info",
                LogLevel.Warning => "Warning",
                LogLevel.Error => "Error",
                LogLevel.Critical => "Critical",
                LogLevel.None => string.Empty,
                _ => null
            };
        }

        private int GetLogLevel(LogLevel level)
        {
            return level switch
            {
                LogLevel.Trace => 6,
                LogLevel.Debug => 5,
                LogLevel.Information => 2,
                LogLevel.Warning => 3,
                LogLevel.Error => 1,
                LogLevel.Critical => 4,
                LogLevel.None => 0,
                _ => 0
            };
        }
    }
}