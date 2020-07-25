using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yadl.Json
{
    public class DictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Dictionary<string, object>);
        }

        public override Dictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);

                if (kvp.Value is Dictionary<string, object> objects)
                {
                    Write(writer, objects, options);
                }

                if (kvp.Value is string sValue)
                {
                    writer.WriteStringValue(sValue);
                    continue;
                }

                if (kvp.Value is short shValue)
                {
                    writer.WriteNumberValue( shValue);
                    continue;
                }

                if (kvp.Value is ushort ushValue)
                {
                    writer.WriteNumberValue(ushValue);
                    continue;
                }

                if (kvp.Value is int iValue)
                {
                    writer.WriteNumberValue(iValue);
                    continue;
                }

                if (kvp.Value is long lValue)
                {
                    writer.WriteNumberValue(lValue);
                    continue;
                }

                if (kvp.Value is ulong ulValue)
                {
                    writer.WriteNumberValue(ulValue);
                    continue;
                }

                if (kvp.Value is float fValue)
                {
                    writer.WriteNumberValue(fValue);
                    continue;
                }

                if (kvp.Value is double dValue)
                {
                    writer.WriteNumberValue(dValue);
                    continue;
                }

                if (kvp.Value is decimal deValue)
                {
                    writer.WriteNumberValue(deValue);
                }
            }

            writer.WriteEndObject();
        }
    }
}