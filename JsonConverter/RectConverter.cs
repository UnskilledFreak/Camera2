using Newtonsoft.Json;
using System;
using Camera2.Utils;
using UnityEngine;

namespace Camera2.JsonConverter
{
    internal class RectConverter : JsonConverter<Rect>
    {
        public override void WriteJson(JsonWriter writer, Rect rect, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.xMin));
            writer.WritePropertyName("y");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.yMin));
            writer.WritePropertyName("width");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.width));
            writer.WritePropertyName("height");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.height));
            writer.WriteEndObject();
        }

        public override Rect ReadJson(JsonReader reader, Type objectType, Rect existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var rect = new Rect();
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
                        rect.x = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "y":
                        rect.y = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "width":
                        rect.width = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "height":
                        rect.height = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                }
            } while (reader.TokenType != JsonToken.EndObject);

            return rect;
        }
    }
}