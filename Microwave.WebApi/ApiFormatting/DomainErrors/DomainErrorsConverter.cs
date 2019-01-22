using System;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.ApiFormatting.DomainErrors
{
    public class DomainErrorsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(DomainError);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is DomainError domainError)) return;

            var obj = new JObject();
            obj.Add(nameof(domainError.ErrorType), domainError.ErrorType);
            if (domainError.Description != null) obj.Add(nameof(domainError.ErrorType), domainError.Description);
            writer.WriteValue(obj);
        }
    }
}