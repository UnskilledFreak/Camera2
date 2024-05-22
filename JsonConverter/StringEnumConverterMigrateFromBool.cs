using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Camera2.JsonConverter
{
    internal class StringEnumConverterMigrateFromBool : StringEnumConverter
    {
        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.TokenType == JsonToken.Boolean 
                ? (bool)reader.Value ? 1 : 0 
                : base.ReadJson(reader, objectType, existingValue, serializer);
        }
    }
}