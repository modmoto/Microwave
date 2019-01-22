using System;
using Microwave.Queries;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.ApiFormatting.ReadModels
{
    public class ReadModelsConverter : JsonConverter
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

            var readModelParsed = JObject.FromObject(readModel);
            readModelParsed.Remove(nameof(ReadModel.GetsCreatedOn));
            writer.WriteRawValue(readModelParsed.ToString());
        }
    }
}