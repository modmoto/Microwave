using System;
using Microwave.Domain.Identities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.ApiFormatting.Identities
{
    public class IdentityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuidIdentity) || objectType == typeof(StringIdentity) || objectType == typeof
                       (Identity);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value is string idValue)
            {
                return Identity.Create(idValue);
            }

            var jObject = JObject.Load(reader);
            var identity = jObject.GetValue(nameof(Identity.Id), StringComparison.OrdinalIgnoreCase).Value<string>();

            return identity != null ? Identity.Create(identity) : null;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var identity = value as Identity;
            writer.WriteValue(identity?.Id);
        }
    }
}