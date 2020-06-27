using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Yadl.Json
{
    public class DictionaryConverter : JsonConverter<IDictionary<string, object>>
    {
        public override bool CanConvert(Type typeToConvert)
        {
            return typeToConvert == typeof(Dictionary<string, object>);
        }

        public override IDictionary<string, object> Read(ref Utf8JsonReader reader, Type typeToConvert,
            JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        public override void Write(Utf8JsonWriter writer, IDictionary<string, object> value,
            JsonSerializerOptions options)
        {
            writer.WriteStartObject();

            foreach (var kvp in value)
            {
                writer.WritePropertyName(kvp.Key);

                if (kvp.Value is IDictionary<string, object>)
                {
                    Write(writer, (IDictionary<string, object>) kvp.Value, options);
                }

                if (kvp.Value is string)
                {
                    writer.WriteStringValue((string)kvp.Value);
                    continue;
                }

                if (kvp.Value is short)
                {
                    writer.WriteNumberValue((short)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is ushort)
                {
                    writer.WriteNumberValue((ushort)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is int)
                {
                    writer.WriteNumberValue((int)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is long)
                {
                    writer.WriteNumberValue((long)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is ulong)
                {
                    writer.WriteNumberValue((ulong)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is float)
                {
                    writer.WriteNumberValue((float)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is double)
                {
                    writer.WriteNumberValue((double)kvp.Value);
                    continue;
                }
                
                if (kvp.Value is decimal)
                {
                    writer.WriteNumberValue((decimal)kvp.Value);
                }
            }
            
            writer.WriteEndObject();
        }
    }
}