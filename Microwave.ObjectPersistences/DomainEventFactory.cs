using System;
using System.Collections.Generic;
using Microwave.Application;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.ObjectPersistences
{
    public interface IDomainEventFactory
    {
        IEnumerable<DomainEventWrapper> Deserialize(string serializeObject);
    }

    public class DomainEventFactory : IDomainEventFactory
    {
        private readonly Dictionary<string, Type> _eventTypeRegistration;

        public DomainEventFactory(EventRegistration eventTypeRegistration)
        {
            _eventTypeRegistration = eventTypeRegistration;
        }

        public IEnumerable<DomainEventWrapper> Deserialize(string serializeObject)
        {
            JsonSerializer serializer = new JsonSerializer();
            serializer.Converters.Add(new IdentityConverter());
            var jArray = JsonConvert.DeserializeObject<JArray>(serializeObject);
            foreach (var jToken in jArray)
            {
                var jObject = (JObject) jToken;
                var value = jObject.GetValue(nameof(DomainEventWrapper.DomainEventType), StringComparison.OrdinalIgnoreCase).Value<string>();
                if (!_eventTypeRegistration.ContainsKey(value)) continue;
                var type = _eventTypeRegistration[value];
                var version = jObject.GetValue(nameof(DomainEventWrapper.Version), StringComparison.OrdinalIgnoreCase).Value<long>();
                var created = jObject.GetValue(nameof(DomainEventWrapper.Created), StringComparison.OrdinalIgnoreCase).Value<long>();
                var domainEventJObject = jObject.GetValue(nameof(DomainEventWrapper.DomainEvent), StringComparison.OrdinalIgnoreCase);
                var domainevent = (IDomainEvent) domainEventJObject.ToObject(type, serializer);

                yield return new DomainEventWrapper
                {
                    Created = created,
                    Version = version,
                    DomainEvent = domainevent
                };
            }
        }
    }

    class IdentityConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(GuidIdentity) || objectType == typeof(StringIdentity) || objectType == typeof
            (Identity);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JObject jObject = JObject.Load(reader);
            var jToken = jObject.GetValue(nameof(Identity.Id), StringComparison.Ordinal);
            if (jToken == null) jToken = jObject.GetValue("id", StringComparison.Ordinal);
            var id = jToken.Value<string>();
            var identity = Identity.Create(id);
            return identity;
        }

        public override bool CanWrite => false;

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
        }
    }
}