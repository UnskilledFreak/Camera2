using System;
using Camera2.Configuration;
using Camera2.Utils;
using Newtonsoft.Json;

namespace Camera2.JsonConverter
{
    internal class ScreenRectConverter : JsonConverter<ScreenRect>
    {
        public override void WriteJson(JsonWriter writer, ScreenRect rect, JsonSerializer serializer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("x");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.X));
            writer.WritePropertyName("y");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.Y));
            writer.WritePropertyName("width");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.Width));
            writer.WritePropertyName("height");
            writer.WriteValue(JsonHelpers.LimitFloatResolution(rect.Height));
            writer.WritePropertyName("locked");
            writer.WriteValue(rect.Locked);
            writer.WriteEndObject();
        }

        public override ScreenRect ReadJson(JsonReader reader, Type objectType, ScreenRect existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            var sr = new ScreenRect(0, 0, 1, 1, false);
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
                        sr.X = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "y":
                        sr.Y = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "width":
                        sr.Width = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "height":
                        sr.Height = (float)reader.ReadAsDecimal().GetValueOrDefault();
                        break;
                    case "locked":
                        sr.Locked = reader.ReadAsBoolean().GetValueOrDefault();
                        break;
                }
            } while (reader.TokenType != JsonToken.EndObject);

            return sr;
        }
    }
}