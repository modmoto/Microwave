using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microwave.WebApi.IdentityFormatting
{
    public class DateTimeOffsetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DateTimeOffset);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(reader.Value is string timeString)) return null;
            if (timeString.Contains(" ")) timeString = timeString.Replace(" ", "+");

            if (DateTimeOffset.TryParseExact(timeString, "o", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = value as DateTimeOffset?;
            writer.WriteValue(identity?.ToString("o"));
        }
    }
}