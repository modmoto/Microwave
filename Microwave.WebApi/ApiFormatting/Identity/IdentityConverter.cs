using System;
using System.Text.RegularExpressions;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.ApiFormatting.Identity
{
    public class IdentityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuidIdentity) || objectType == typeof(StringIdentity) || objectType == typeof
                       (Domain.Identity);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
           return Domain.Identity.Create(reader.Value as string);
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = value as Domain.Identity;
            writer.WriteValue(identity?.Id);
        }
    }
}