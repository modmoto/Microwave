using System;
using System.Collections.Generic;
using Microwave.Application;
using Microwave.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.Queries
{
    public class DomainEventFactory
    {
        private readonly Dictionary<string, Type> _eventTypeRegistration;

        public DomainEventFactory(Dictionary<string, Type> eventTypeRegistration)
        {
            _eventTypeRegistration = eventTypeRegistration;
        }

        public IEnumerable<DomainEventWrapper> Deserialize(string serializeObject)
        {
            var jArray = JsonConvert.DeserializeObject<JArray>(serializeObject);
            foreach (var jToken in jArray)
            {
                var jObject = (JObject) jToken;
                var value = jObject.GetValue(nameof(DomainEventWrapper.DomainEventType)).Value<string>();
                if (!_eventTypeRegistration.ContainsKey(value)) yield break;
                var type = _eventTypeRegistration[value];
                var version = jObject.GetValue(nameof(DomainEventWrapper.Version)).Value<long>();
                var created = jObject.GetValue(nameof(DomainEventWrapper.Created)).Value<long>();
                var domainEventJObject = jObject.GetValue(nameof(DomainEventWrapper.DomainEvent));
                var domainevent = (IDomainEvent) domainEventJObject.ToObject(type);

                yield return new DomainEventWrapper
                {
                    Created = created,
                    Version = version,
                    DomainEvent = domainevent
                };
            }
        }
    }
}