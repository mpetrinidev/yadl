using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Extensions.Options;
using Yadl;
using Yadl.Abstractions;
using Yadl.Common;

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

            CompleteMessage(message, _options.GlobalFields);
            if (!_processor.ChannelWriter.TryWrite(message))
            {
                return;
            }
        }

        private void CompleteMessage(YadlMessage message, Dictionary<string, object> globalFields)
        {
            if (globalFields != null && globalFields.Count > 0)
            {
                foreach (var globalField in globalFields)
                {
                    switch (globalField.Key.ToLower())
                    {
                        case "ep_origen":
                        case "eporigen":
                        case "ip_origen":
                        case "iporigen":
                            message.IpOrigen = globalField.Value.ToString();
                            continue;
                        case "ep_destino":
                        case "epdestino":
                        case "ip_destino":
                        case "ipdestino":
                            message.IpDestino = globalField.Value.ToString();
                            continue;
                        case "datos":
                            message.Datos = globalField.Value.ToString();
                            continue;
                        case "tipo_obj":
                        case "tipoobj":
                            message.TipoObj = globalField.Value.ToString();
                            continue;
                        case "id_obj":
                        case "idobj":
                            message.IdObj = GetIdObj(globalField.Value);
                            continue;
                        case "id_obj_hash":
                        case "idobjhash":
                            message.IdObjHash = globalField.Value.ToString();
                            continue;
                        case "cod_resp":
                        case "codresp":
                            message.CodRespuesta = globalField.Value.ToString();
                            continue;
                        case "sec_client":
                        case "secclient":
                            message.SecClient = globalField.Value.ToString();
                            continue;
                        case "sec_banco":
                        case "secbanco":
                        case "sec_bco":
                        case "secbco":
                            message.SecBanco = globalField.Value.ToString();
                            continue;
                        case "adddt":
                            message.AddDt = GetAddDt(globalField.Value);
                            continue;
                    }
                }
            }

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

            _scopeProvider?.ForEachScope((scope, mes) =>
            {
                var pairs = (Dictionary<string, object>) scope;
                foreach (var pair in pairs)
                {
                    switch (pair.Key)
                    {
                    }
                }
            }, message);
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