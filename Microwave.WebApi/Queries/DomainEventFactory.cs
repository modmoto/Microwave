using System;
using System.Collections.Generic;
using Microwave.Queries;
using Microwave.Queries.Ports;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microwave.WebApi.Queries
{
    public interface IDomainEventFactory
    {
        IEnumerable<SubscribedDomainEventWrapper> Deserialize(string serializeObject);
    }

    public class DomainEventFactory : IDomainEventFactory
    {
        private readonly Dictionary<string, Type> _eventTypeRegistration;

        public DomainEventFactory(EventRegistration eventTypeRegistration)
        {
            _eventTypeRegistration = eventTypeRegistration;
        }

        public IEnumerable<SubscribedDomainEventWrapper> Deserialize(string serializeObject)
        {
            JsonSerializer serializer = new JsonSerializer();
            var jArray = JsonConvert.DeserializeObject<JArray>(serializeObject);
            foreach (var jToken in jArray)
            {
                var jObject = (JObject) jToken;
                var value = jObject.GetValue(nameof(SubscribedDomainEventWrapper.DomainEventType), StringComparison.OrdinalIgnoreCase).Value<string>();
                if (!_eventTypeRegistration.ContainsKey(value)) continue;
                var type = _eventTypeRegistration[value];
                var version = jObject.GetValue(nameof(SubscribedDomainEventWrapper.EntityStreamVersion), StringComparison.OrdinalIgnoreCase).Value<long>();
                var globalVersion = jObject.GetValue(nameof(SubscribedDomainEventWrapper.OverallVersion), StringComparison.OrdinalIgnoreCase).Value<long>();
                var domainEventJObject = jObject.GetValue(nameof(SubscribedDomainEventWrapper.DomainEvent), StringComparison
                .OrdinalIgnoreCase);
                var domainevent = (ISubscribedDomainEvent) domainEventJObject.ToObject(type, serializer);

                if (domainevent.EntityId == null) throw new DomainEventNotAssignableToEntityException(domainevent);
                yield return new SubscribedDomainEventWrapper
                {
                    OverallVersion = globalVersion,
                    EntityStreamVersion = version,
                    DomainEvent = domainevent
                };
            }
        }
    }
}