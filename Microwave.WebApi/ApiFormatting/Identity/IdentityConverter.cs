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
            if (reader.Value is string idValue)
            {
                return Domain.Identity.Create(idValue);
            }

            var jObject = JObject.Load(reader);
            var identity = jObject.GetValue(nameof(Domain.Identity.Id), StringComparison.OrdinalIgnoreCase).Value<string>();

            return identity != null ? Domain.Identity.Create(identity) : null;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = value as Domain.Identity;
            writer.WriteValue(identity?.Id);
        }
    }
}