using System;
using Camera2.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Camera2.JsonConverter
{
    internal class Vector3Converter : JsonConverter<Vector3>
    {
        public override void WriteJson(JsonWriter writer, Vector3 vec, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(vec.x));
            writer.WritePropertyName("y");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(vec.y));
            writer.WritePropertyName("z");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(vec.z));
            writer.WriteEndObject();
        }

        public override Vector3 ReadJson(JsonReader reader, Type objectType, Vector3 existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var vec = new Vector3();
            do
            {
                reader.Read();
                if (reader.TokenType != JsonToken.PropertyName)
                {
                    continue;
                }

                var property = reader.Value?.ToString() ?? "";
                switch (property)
                {
                    case "x":
                        vec.x = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "y":
                        vec.y = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "z":
                        vec.z = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                }
            } while (reader.TokenType != JsonToken.EndObject);

            return vec;
        }
    }
}