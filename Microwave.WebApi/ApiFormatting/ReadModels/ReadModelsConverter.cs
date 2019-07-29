using System;
using Microwave.Queries;
using Microwave.WebApi.ApiFormatting.DateTimeOffsets;
using Microwave.WebApi.ApiFormatting.Identities;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.ApiFormatting.ReadModels
{
    internal class ReadModelsConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return typeof(ReadModel).IsAssignableFrom(objectType);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return reader.Value;
        }

        public override bool CanWrite => true;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            if (!(value is ReadModel readModel)) return;

            var serializeObject = JsonConvert.SerializeObject(readModel,
                new IdentityConverter(),
                new DateTimeOffsetConverter());

            var jObject = JObject.Parse(serializeObject);
            jObject.Remove(nameof(ReadModel.GetsCreatedOn));
            writer.WriteRawValue(jObject.ToString(Formatting.Indented));
        }
    }
}