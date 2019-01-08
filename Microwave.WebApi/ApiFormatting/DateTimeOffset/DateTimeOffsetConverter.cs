using System;
using System.Globalization;
using Newtonsoft.Json;

namespace Microwave.WebApi.ApiFormatting.DateTimeOffset
{
    public class DateTimeOffsetConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(System.DateTimeOffset);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (!(reader.Value is string timeString)) return null;
            if (timeString.Contains(" ")) timeString = timeString.Replace(" ", "+");

            if (System.DateTimeOffset.TryParseExact(timeString, "o", CultureInfo.InvariantCulture,
                DateTimeStyles.None, out var result))
            {
                return result;
            }

            return null;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = value as System.DateTimeOffset?;
            writer.WriteValue(identity?.ToString("o"));
        }
    }
}